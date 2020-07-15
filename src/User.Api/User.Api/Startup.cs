using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Consul;
using DotNetCore.CAP;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using User.Api.Data;
using User.Api.Entity.Dtos;
using User.Api.Filters;

namespace User.Api
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
            services.AddControllers(options => { options.Filters.Add(typeof(GlobalExceptionFilter)); })
                .AddNewtonsoftJson((options) =>
                {
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                });
            //添加Jwt认证
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.RequireHttpsMetadata = false;
                    //对应的Api资源名称
                    options.Audience = "user_api";
                    options.Authority = "http://192.168.1.4";
                });

            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            services.AddSingleton<IConsulClient>(p => new ConsulClient(cfg =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;

                if (!string.IsNullOrEmpty(serviceConfiguration.Consul.HttpEndpoint))
                {
                    cfg.Address = new Uri(serviceConfiguration.Consul.HttpEndpoint);
                }
            }));

            services.AddDbContext<UserDbContext>(options =>
            {
                options.UseMySQL(Configuration.GetConnectionString("MysqlUser"));
            });

            services.AddCap(options =>
            {
                options.UseEntityFramework<UserDbContext>()
                    .UseMySql(mysqlConfig =>
                    {
                        mysqlConfig.ConnectionString = Configuration["ConnectionStrings:MysqlUser"];
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
                    d.CurrentNodePort = 8850;
                    d.NodeId = "1";
                    d.NodeName = "CAP User Node";
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env
            ,IHostApplicationLifetime lifetimes,
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
