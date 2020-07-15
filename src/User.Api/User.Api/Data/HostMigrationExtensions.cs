using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace User.Api.Data
{
    public static class HostMigrationExtensions
    {
        /// <summary>
        /// 初始化database方法
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="host"></param>
        /// <param name="sedder"></param>
        /// <returns></returns>
        public static IHost MigrateDbContext<TContext>(this IHost host, Action<TContext, IServiceProvider> sedder,int? retry = 0)
            where TContext : UserDbContext
        {
            var retryForAvaiability = retry.Value;
            //创建数据库实例在本区域有效
            using (var scope = host.Services.CreateScope())
            {

                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetService<TContext>();
                logger.LogDebug("开始执行初始化");
                try
                {
                    context.Database.Migrate();//初始化database
                }
                catch (Exception e)
                {
                    logger.LogDebug("错误: "+e);
                    context.Database.Migrate();
                }
                sedder(context, services);
                logger.LogInformation($"执行DbContext{typeof(TContext).Name} seed 成功");
            }
            return host;
        }
    }
}