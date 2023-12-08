using Core.Database;
using Core.Http;
using Core.Interface;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
//using Notification.Interface;

namespace Core.BL
{
    public class CoreServiceCollection
    {

        private IServiceProvider _serviceProvider;

        private IAuthService _authService;
        private ICacheService _cacheService;
        private IConfigService _configService;
        private ILogService _logService;
        private IDatabaseService _databaseService;
        private IHttpService _httpService;
        private IMemoryCacheService _memoryCacheService;
        public IAuthService AuthService()
        {
            if(_authService == null)
            {
                _authService = GetServieByType<IAuthService>();
            }
            return _authService;
        }


        private T GetServieByType<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }
        public ICacheService CacheService()
        {
            if (_cacheService == null)
            {
                _cacheService = GetServieByType<ICacheService>();
            }
            return _cacheService;
        }
        public IMemoryCacheService MemoryCacheService()
        {
            if (_memoryCacheService == null)
            {
                _memoryCacheService = GetServieByType<IMemoryCacheService>();
            }
            return _memoryCacheService;
        }
        public IConfigService ConfigService()
        {
            if(_configService == null)
            {
                _configService = GetServieByType<IConfigService>();
            }
            return _configService;
        }
        public ILogService LogService()
        {
            if (_logService == null)
            {
                _logService = GetServieByType<ILogService>();
            }
            return _logService;

        }
        // public IOptionService OptionService{ set; get; }
        public IDatabaseService DatabaseService()
        {
            if (_databaseService == null)
            {
                _databaseService = GetServieByType<IDatabaseService>();
            }
            return _databaseService;
        }
        public IHttpService HttpService()
        {
            if (_httpService == null)
            {
                _httpService = GetServieByType<IHttpService>();
            }
            return _httpService;
        }
        //public IPushNotificationService PushNotificationService{ set; get; }
        //public INotificationService NotificationService{ set; get; }
        //public INotificationCenterService NotificationCenterService{ set; get; }
        //public ISessionBL SessionBl;

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
        public CoreServiceCollection(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

    }
}