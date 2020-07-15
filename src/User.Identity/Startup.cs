using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DnsClient;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience.Http;
using User.Identity.Authentication;
using User.Identity.Dtos;
using User.Identity.Infrastructure;
using User.Identity.Services;

namespace User.Identity
{
    public class Startup
    {
        private readonly IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServer()
                .AddExtensionGrantValidator<SmsAuthCodeValidator>()
                .AddDeveloperSigningCredential()
                .AddInMemoryClients(Config.GetClients())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddProfileService<UserProfileService>();
            services.AddControllers();

            #region 服务发现
            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            services.AddSingleton<IDnsQuery>(p =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                return new LookupClient(serviceConfiguration.Consul.DnsEndpoint.ToIPEndPoint());
            });
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            //获取注册全局ResilienceClientFactory
            services.AddSingleton(typeof(ResilienceClientFactory), sp =>
            {
                var logger = sp.GetService<ILogger<ResilienceHttpClient>>();
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
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
            services.AddScoped<IUserService, UserService>()
                .AddScoped<IAuthCodeService, TestAuthCodeService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseIdentityServer();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
