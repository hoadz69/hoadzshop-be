using System;
using System.Collections.Generic;
using System.IO;
using Core.Contant;
using Core.Interface;
using Core.Ultitily;
using Microsoft.Extensions.Logging;
//using NLog;
//using NLog.Web;

namespace Core.Services
{
    // phien ban hien tại đã khác
    internal class LogService: ILogService
    {
//        private static Logger _currentLogger;

//        private static Logger CurrentLogger
//        {
//            get
//            {
//                if (_currentLogger == null)
//                {
//                    SetLoggerByEnvironment();
//                }

//                return _currentLogger;
//            }
//        }

//        private static void SetLoggerByEnvironment()
//        {
//            var configName = $"NLog.{StartupParameter.EnvironmentName}.config";
//#if DEBUG
//            configName = Path.Combine((AppDomain.CurrentDomain.BaseDirectory), configName);
            
//#endif
//            if (!File.Exists(configName))
//            {
//                configName = "NLog.config";
//            }

//            _currentLogger = NLogBuilder.ConfigureNLog(configName).GetCurrentClassLogger();
//        }

//        private readonly IConfigService _configService;
//        private readonly IAuthService _authService;

//        public LogService(IConfigService configService, IAuthService authService)
//        {
//            _configService = configService;
//            _authService = authService;
//        }
        
        
//        public void LogTrace(string message, Dictionary<string, object> properties = null)
//        {
//            var logEventInfo = CreateLogEventInfo(LogLevel.Trace, message, properties);
//            CurrentLogger.Log(logEventInfo);
//        }

//        public void LogDebug(string message, Dictionary<string, object> properties = null)
//        {
//            var logEventInfo = CreateLogEventInfo(LogLevel.Debug, message, properties);
//            CurrentLogger.Log(logEventInfo);
//        }

//        public void LogInfo(string message, Dictionary<string, object> properties = null)
//        {
//            var logEventInfo = CreateLogEventInfo(LogLevel.Info, message, properties);
//            CurrentLogger.Log(logEventInfo);
//        }

//        public void LogWarning(string message, Dictionary<string, object> properties = null)
//        {
//            var logEventInfo = CreateLogEventInfo(LogLevel.Warn, message, properties);
//            CurrentLogger.Log(logEventInfo);
//        }

//        public void LogError(string message, Dictionary<string, object> properties = null)
//        {
//            var logEventInfo = CreateLogEventInfo(LogLevel.Error, message, properties);
//            CurrentLogger.Log(logEventInfo);
//        }

//        public void LogError(Exception exception, string message, Dictionary<string, object> properties = null)
//        {
//            var logEventInfo = CreateLogEventInfo(LogLevel.Error, message, properties,ex: exception);
//            CurrentLogger.Log(logEventInfo);
//        }

//        public void LogFatal(Exception exception, string message, Dictionary<string, object> properties = null)
//        {
//            var logEventInfo = CreateLogEventInfo(LogLevel.Fatal, message, properties, ex: exception);
//            CurrentLogger.Log(logEventInfo);
//        }

//        public void LogFatal(string message, Dictionary<string, object> properties = null)
//        {
//            var logEventInfo = CreateLogEventInfo(LogLevel.Fatal, message, properties);
//            CurrentLogger.Log(logEventInfo);
//        }

//        private LogEventInfo CreateLogEventInfo(LogLevel logLevel, string message,
//            Dictionary<string, object> properties, Exception ex = null)
//        {
//            var info= new LogEventInfo()
//            {
//                Level = logLevel,
//                Message = message,
//                Exception = ex
//            };
//            info.Properties.Add(AppSettingsKey.ApplicationCode,Core.StartupParameter.ApplicationCode ?? _configService.GetAppSetting(AppSettingsKey.ApplicationCode));
//            var tenantId = _authService.GetTenantId();
//            info.Properties.Add(Keys.TenantId,((!tenantId.ToString().Equals(CommonConstant.INVALID_GUID,StringComparison.OrdinalIgnoreCase))? tenantId.ToString() : string.Empty));
//            if (properties != null&& properties.Count > 0)
//            {
//                foreach (var item in properties)
//                {
//                    string propertyValue = string.Empty;
//                    if (item.Value != null)
//                    {
//                        if (item.Value is string)
//                        {
//                            propertyValue = item.Value.ToString();
//                        }
//                        else
//                        {
//                            propertyValue = Converter.Serialize(item.Value);
//                        }
//                    }
//                    if (info.Properties.ContainsKey(item.Key))
//                    {
//                        info.Properties[item.Key] = propertyValue;
//                    }
//                    else
//                    {
//                        info.Properties.Add(item.Key,propertyValue);
//                    }
                    
//                }
//            }

//            return info;
//        }
    }
}