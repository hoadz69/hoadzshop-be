using Core.Interface;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
namespace Core
{
    public static class StartupExtension
    {
        public static void UseCoreStartup(this IServiceCollection services)
        {
            services.AddTransient<IDistributedCache, CustomMemoryDistributedCache>();
            services.AddTransient<IMemoryCacheService, MemoryCacheService>();
            services.AddTransient<IMemoryDistributedCache, CustomMemoryDistributedCache>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<ICacheService, CacheService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IConfigService, ConfigService>();
            services.AddTransient<ILogService, LogService>();
            //services.UseLicenseService(configuration);
            //services.UseMonitorService(configuration);

            //services.AddTransient<ISessionBL, SessionBL>();


            //services.AddTransient<CoreServiceCollection>();
        }

         

    }
}
