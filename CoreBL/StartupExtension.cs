using Core.Database;
using Core.Http;
using Core.Interface;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
namespace Core.BL
{
    public static class StartupExtension
    {

        private static bool IsInitialized { get; set; } = false;

        public static void UseCoreBL(this IServiceCollection services, IConfiguration configuration)
        {
            if (!IsInitialized)
            {

                //services.UseCoreService(configuration);

                //services.UseOptionService(configuration);
                //services.UseDatabaseService(configuration);
                //services.UseExchangeDataService(configuration);
                ////services.UseHttpService(configuration);

                //services.UseNotificationService(configuration);
                //services.UsePushNotificationService(configuration);
                //services.UseNotificationCenterService(configuration);



                services.UseCoreStartup();
                services.UseDatabaseService();
                services.UseHttpService(configuration);
                UseAuditLogService(ref services, configuration);

                //services.UseLicenseService(configuration);
                //services.UseMonitorService(configuration);

                //services.AddTransient<ISessionBL, SessionBL>();
                services.AddTransient<CoreServiceCollection, CoreServiceCollection>();
              
                //services.AddTransient<CoreServiceCollection>();
                services.AddTransient<IBaseBL, BaseBL>();
            }
        }

        /// <summary>
        /// Sử dụng audit log theo config type
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        private static void UseAuditLogService(ref IServiceCollection services, IConfiguration configuration)
        {
            int logType = 0;
            var auditingLogConfig = configuration.GetSection("AppSettings:AuditingLogType").Value;
            if (!string.IsNullOrWhiteSpace(auditingLogConfig))
            {
                Int32.TryParse(auditingLogConfig, out logType);
            }

            //switch (logType)
            //{
            //    //1: sử dụng mongo
            //    case (int)AuditLogType.MongoDB:
            //        services.UseMongoAuditLogService(configuration);
            //        break;
            //    default:
            //        services.UseMISAAuditingLogService(configuration);
            //        break;
            //}


        }

        //public static void AddFixedConnectionString(IServiceProvider serviceProvider, string appCode, string connectionStringKey)
        //{
        //    var databaseService = serviceProvider.GetService<IDatabaseService>();
        //    databaseService.AddFixedConnectionString(appCode, connectionStringKey);
        //}

        private static System.Threading.Timer _timer;
        public static void UseGCCollectService(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                var useGCCollect = configuration.GetSection("AppSettings:UseGCCollect").Value;
                if (string.IsNullOrWhiteSpace(useGCCollect) || !bool.FalseString.Equals(useGCCollect, StringComparison.OrdinalIgnoreCase))
                {
                    int gcCollectInterval = 15;
                    var gcCollectIntervalValue = configuration.GetSection("AppSettings:GCCollectInterval").Value;
                    if (!string.IsNullOrWhiteSpace(gcCollectIntervalValue))
                    {
                        Int32.TryParse(gcCollectIntervalValue, out gcCollectInterval);
                    }

                    _timer = new System.Threading.Timer((object state) =>
                    {
                        try
                        {
                            GC.Collect();
                        }
                        catch { }
                    }, null, TimeSpan.FromMinutes(gcCollectInterval), TimeSpan.FromMinutes(gcCollectInterval));
                }
            }
            catch
            { }
        }

        //public static void StartHandleSystemEvents<TInterface>()
        //{
        //    try
        //    {
        //        var handledBL = StartupParameter.GetServiceInstanceByType<TInterface>();
        //        (handledBL as BaseBL)?.HandleSystemEvents();
        //    }
        //    catch
        //    { }
        //}

        //public static void StopHandleSystemEvents<TInterface>()
        //{
        //    try
        //    {
        //        var handledBL = StartupParameter.GetServiceInstanceByType<TInterface>();
        //        (handledBL as BaseBL)?.StopHandleSystemEvents();
        //    }
        //    catch
        //    { }
        //}

    }
}
