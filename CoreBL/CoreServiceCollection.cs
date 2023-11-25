using Core.Database;
using Core.Http;
using Core.Interface;
using Core.Services;
//using Notification.Interface;

namespace Core.BL
{
    public class CoreServiceCollection
    {
        public IAuthService AuthService { set; get; }
        public ICacheService CacheService { set; get; }
        public IMemoryCacheService MemoryCacheService{ set; get; }
        public IConfigService ConfigService{ set; get; }
        public ILogService LogService{ set; get; }
        // public IOptionService OptionService{ set; get; }
        public IDatabaseService DatabaseService{ set; get; }
        public IHttpService HttpService{ set; get; }
        //public IPushNotificationService PushNotificationService{ set; get; }
        //public INotificationService NotificationService{ set; get; }
        //public INotificationCenterService NotificationCenterService{ set; get; }
        public ISessionBL SessionBl;

        //public CoreServiceCollection(ISessionBL sessionBl, IAuthService authService, ICacheService cacheService, IMemoryCacheService memoryCacheService, IConfigService configService, ILogService logService, IDatabaseService databaseService, IHttpService httpService, IPushNotificationService pushNotificationService, INotificationService notificationService, INotificationCenterService notificationCenterService)
        //{
        //    SessionBl = sessionBl;
        //    AuthService = authService;
        //    CacheService = cacheService;
        //    MemoryCacheService = memoryCacheService;
        //    ConfigService = configService;
        //    LogService = logService;
        //    // OptionService = optionService;
        //    DatabaseService = databaseService;
        //    HttpService = httpService;
        //    PushNotificationService = pushNotificationService;
        //    NotificationService = notificationService;
        //    NotificationCenterService = notificationCenterService;
        //}
        public CoreServiceCollection(ISessionBL sessionBl, IAuthService authService, ICacheService cacheService, IMemoryCacheService memoryCacheService, IConfigService configService, ILogService logService, IDatabaseService databaseService, IHttpService httpService)
        {
            SessionBl = sessionBl;
            AuthService = authService;
            CacheService = cacheService;
            MemoryCacheService = memoryCacheService;
            ConfigService = configService;
            LogService = logService;
            // OptionService = optionService;
            DatabaseService = databaseService;
            HttpService = httpService; 
        }

    }
}