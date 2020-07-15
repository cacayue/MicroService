using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using DnsClient;
using DotNetCore.CAP;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recommends.API.Data;
using Recommends.API.Dtos;
using Recommends.API.Infrastructure;
using Recommends.API.IntegrationEventHandlers;
using Recommends.API.Services;
using Resilience.Http;

namespace Recommends.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //添加Jwt认证
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.RequireHttpsMetadata = false;
                    //对应的Api资源名称
                    options.Audience = "recommend_api";
                    options.Authority = "http://192.168.1.4";
                    options.SaveToken = true;
                });
            services
                .AddScoped<IUserService, UserService>()
                .AddScoped<IContactService,ContactService>();
            #region 服务发现
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
            services.AddHttpContextAccessor();
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

            #endregion

            services.AddDbContext<RecommendDbContext>(options =>
            {
                options.UseMySQL(Configuration["ConnectionStrings:MysqlRecommend"]);
            });

            services.AddControllers();
         
            //注入对应的cap服务
            services.AddTransient<IProjectCreatedEventHandler, ProjectCreatedEventHandler>();

            services.AddCap(options =>
            {
                options.UseEntityFramework<RecommendDbContext>()
                    .UseMySql(mysqlConfig =>
                    {
                        mysqlConfig.ConnectionString = Configuration["ConnectionStrings:MysqlRecommend"];
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
                        d.CurrentNodePort = 8880;
                        d.NodeId = "4";
                        d.NodeName = "CAP Recommend Node";
                    });
            });
        }

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

            app.UseCapDashboard();
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
