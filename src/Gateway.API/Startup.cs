using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Gateway.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var authenticationProviderKey = "finbook";
            services.AddAuthentication()
                .AddIdentityServerAuthentication(authenticationProviderKey, options =>
                {
                    options.RequireHttpsMetadata = false; //�Ƿ�����https
                    options.Authority = $"http://192.168.1.4:8060";//������Ȩ��֤�ĵ�ַ
                    options.ApiName = "gateway_api"; //��Դ���ƣ�����֤������ע�����Դ�б������е�apiResourceһ��
                    options.ApiSecret = "secret";
                    options.SupportedTokens = SupportedTokens.Both;
                });

            services.AddOcelot();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
            }

            //app.UseAuthentication();
            app.UseOcelot().Wait();

        }
    }
}
