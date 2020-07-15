using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Consul;
using Contact.API.Data;
using Contact.API.Data.Repository;
using Contact.API.Dtos;
using Contact.API.Infrastructure;
using Contact.API.IntegrationEvents.EventHandling;
using Contact.API.Models;
using Contact.API.Services;
using DnsClient;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience.Http;

namespace Contact.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // 获取配置文件 的 MongoSettings
            services.Configure<MongoDatabaseSettings>(
                Configuration.GetSection(nameof(MongoDatabaseSettings)));


            //添加Jwt认证
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.RequireHttpsMetadata = false;
                    //对应的Api资源名称
                    options.Audience = "contact_api";
                    options.Authority = "http://192.168.1.4";
                    options.SaveToken = true;
                });
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            #region 服务发现
            services.AddHttpContextAccessor();
            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            services.AddSingleton<IConsulClient>(p => new ConsulClient(cfg =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;

                if (!string.IsNullOrEmpty(serviceConfiguration.Consul.HttpEndpoint))
                {
                    cfg.Address = new Uri(serviceConfiguration.Consul.HttpEndpoint);
                }
            }));
            services.AddSingleton<IDnsQuery>(p =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                return new LookupClient(serviceConfiguration.Consul.DnsEndpoint.ToIPEndPoint());
            });

            #endregion

            #region Polly

            //获取注册全局ResilienceClientFactory
            services.AddSingleton(typeof(ResilienceClientFactory), sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ResilienceHttpClient>>();
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var retryCount = 5;
                var exceptionCountAllowedBeforeBreaking = 5;
                bool.TryParse(Configuration["ResilienceFactoryConfig:UseResilienceClientConfig"], out var enable);
                if (enable)
                {
                    retryCount =
                        int.Parse(Configuration["ResilienceFactoryConfig:RetryCount"]);
                    exceptionCountAllowedBeforeBreaking =
                        int.Parse(Configuration["ResilienceFactoryConfig:ExceptionCountAllowedBeforeBreaking"]);
                }
                return new ResilienceClientFactory(logger, httpContextAccessor, retryCount,
                    exceptionCountAllowedBeforeBreaking);
            });
            //获取注册全局HttpClient
            services.AddSingleton<IHttpClient>(sp =>
                sp.GetRequiredService<ResilienceClientFactory>().GetResilienceHttpClient());
            services.AddScoped<ContactContext>()
                .AddScoped<IContactApplyRequestRepository, MongoContactApplyRequestRepository>()
                .AddScoped<IContactRepository, MongoContactRepository>()
                .AddScoped<IUserService, UserService>();

            #endregion

            services.AddControllers()
                .AddNewtonsoftJson();

            //注意: 注入的服务需要在 `services.AddCap()` 之前
            services.AddTransient<ISubscriberService, UserProfileChangedEventHandler>();

            services.AddCap(options =>
            {
                options
                    .UseMySql(mysqlConfig =>
                    {
                        mysqlConfig.ConnectionString = Configuration["ConnectionStrings:MysqlContact"];
                    })
                    .UseRabbitMQ(o =>
                    {
                        int.TryParse(Configuration["RabbitMqConfig:Port"], out var port);
                        o.HostName = Configuration["RabbitMqConfig:HostName"];
                        o.UserName = Configuration["RabbitMqConfig:UserName"];
                        o.Password = Configuration["RabbitMqConfig:Password"];
                        o.Port = port;
                    })
                    .UseDashboard()
                    .UseDiscovery(d =>
                    {
                        d.DiscoveryServerHostName = "192.168.65.173";
                        d.DiscoveryServerPort = 8500;
                        d.CurrentNodeHostName = "192.168.1.4";
                        d.CurrentNodePort = 8860;
                        d.NodeId = "2";
                        d.NodeName = "CAP Contact Node";
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime lifetimes,
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseRouting();
            //添加认证
            app.UseAuthentication();
            //添加授权
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            //启动注册到Consul
            lifetimes.ApplicationStarted.Register(() =>
            {
                Register(app, serviceOptions, consul);
            });
            //关闭从Consul移除
            lifetimes.ApplicationStopping.Register(() =>
            {
                DeRegister(app, serviceOptions, consul);
            });
        }

        private void Register(IApplicationBuilder app,
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)
        {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach (var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";
                var httpCheck = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                    Interval = TimeSpan.FromSeconds(30),
                    HTTP = new Uri(address, "HealthCheck").OriginalString
                };

                var registration = new AgentServiceRegistration()
                {
                    Checks = new[] { httpCheck },
                    Address = address.Host,
                    ID = serviceId,
                    Name = serviceOptions.Value.ServiceName,
                    Port = address.Port
                };

                consul.Agent.ServiceRegister(registration).GetAwaiter().GetResult();
            }
        }

        private void DeRegister(IApplicationBuilder app,
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)
        {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach (var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                consul.Agent.ServiceDeregister(serviceId).GetAwaiter().GetResult();
            }
        }
    }
}
