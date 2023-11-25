using System;
using System.Collections.Generic;
using System.Net.Http;
using Core.Contant;
using Core.Interface;
using Core.Services;
using Core.Ultitily;
using Microsoft.AspNetCore.Http;

namespace Core.Http
{
    public class BaseHttpClient
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IConfigService _configService;
        protected readonly ILogService _logService;

        public BaseHttpClient(IHttpContextAccessor httpContextAccessor, IConfigService configService, ILogService logService)
        {
            _httpContextAccessor = httpContextAccessor;
            _configService = configService;
            _logService = logService;
        }

        public void SetRequestHeader(HttpRequestMessage requestMessage, Dictionary<string, string> headers)
        {
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    requestMessage.Headers.Add(header.Key,header.Value);   
                }
            }
        }

        public string GetApiUrl(string apiUrlKey)
        {
            return _configService.GetApiUrl(apiUrlKey);
        }

        public string GetInternalApiToken()
        {
            return _configService.GetAppSetting(AppSettingsKey.InternalApiToken);
        }

        public string GetAuthorizationToken()
        {
            return _httpContextAccessor?.HttpContext?.Request?.Headers[Keys.Authorization] + "";
        }

        public void LogRequest(HttpRequestMessage requestMessage)
        {
            if (requestMessage != null)
            {
                var logContent = "LogRequest server-side:";
                logContent += Environment.NewLine + $"URI:{requestMessage.RequestUri.AbsoluteUri}";
                logContent += Environment.NewLine + $"Method:{requestMessage.Method.Method}";
                try
                {
                    logContent += Environment.NewLine + $"Header:{Converter.Serialize(requestMessage.Headers)}";
                    logContent += Environment.NewLine + $"Content:{Converter.Serialize(requestMessage.Content)}";
                }
                catch (Exception ex)
                {
                    
                }
                //_logService.LogTrace(logContent);
            }
        }

        public string GetSessionId()
        {
            return _httpContextAccessor?.HttpContext?.Request?.Headers[Keys.SessionId] + "";
        }
    }
}