using System;
using System.Text;
using System.Threading.Tasks;
using Core.Contant;
using Core.Interface;
using Core.Ultitily;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core.Services
{
    internal class CacheService : BaseCacheService , ICacheService
    {
        public CacheService(IDistributedCache cache, IConfigService configService,ILogService logService) : base(configService : configService,logService : logService)
        {
            _cache = cache;
        }
    }

    internal class MemoryCacheService : BaseCacheService, IMemoryCacheService
    {
        public MemoryCacheService(IMemoryDistributedCache cache, IConfigService configService, ILogService logService) : base(configService : configService, logService : logService)
        {
            _cache = cache;
        }
    }

    internal class CustomMemoryDistributedCache : MemoryDistributedCache, IMemoryDistributedCache
    {
        public CustomMemoryDistributedCache(IOptions<MemoryDistributedCacheOptions> optionsAccessor) : base(optionsAccessor)
        {
            
        }

        public CustomMemoryDistributedCache(IOptions<MemoryDistributedCacheOptions> optionsAccessor,
            ILoggerFactory loggerFactory) : base(optionsAccessor, loggerFactory)
        {
            
        }
    }

    internal class BaseCacheService
    {
        protected IDistributedCache _cache;
        private readonly IConfigService _configService;
        private readonly ILogService _logService;

        public BaseCacheService( IConfigService configService, ILogService logService)
        {
            _configService = configService;
            _logService = logService;
        }
        /// <summary>
        /// Thêm object vào cache
        /// </summary>
        /// <param name="key">cache key</param>
        /// <param name="value">object cần cache</param>
        /// <param name="isAbsoluteExpiration"></param>
        /// <param name="isAppendAppCodeToKey"></param>
        /// <returns></returns>
        public async Task Set(string key, object value, bool isAbsoluteExpiration = false, bool isAppendAppCodeToKey = true)
        {
            await Set(key, value, TimeSpan.FromMinutes(20), isAbsoluteExpiration, isAppendAppCodeToKey);
        }
        /// <summary>
        /// Thêm object vào cache, có thời gian hết hạn
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeout">thời gian hết hạn</param>
        /// <param name="isAbsoluteExpiration"></param>
        /// <param name="isAppendAppCodeToKey"></param>
        /// <returns></returns>
        public async Task Set(string key, object value, TimeSpan timeout, bool isAbsoluteExpiration = false,
            bool isAppendAppCodeToKey = true)
        {
            key = ProcessCacheKey(key, isAppendAppCodeToKey);
            var option= new DistributedCacheEntryOptions();
            if (isAbsoluteExpiration)
            {
                option.SetAbsoluteExpiration(timeout);
            }
            else
            {
                option.SetSlidingExpiration(timeout);
            }

            try
            {
                var jsonValue = Converter.Serialize(value);
                await _cache.SetAsync(key, Encoding.UTF8.GetBytes(jsonValue), option);
            }
            catch (Exception ex)
            {
                //_logService.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// Xoá giá trị trong cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isAppendAppCodeToKey"></param>
        /// <returns></returns>
        public async Task Delete(string key, bool isAppendAppCodeToKey = true)
        {
            key = ProcessCacheKey(key, isAppendAppCodeToKey);
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                //_logService.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// lấy object trong cache
        /// </summary>
        /// <param name="key">cache key</param>
        /// <param name="isAppendAppCodeToKey"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> Get<T>(string key, bool isAppendAppCodeToKey = true)
        {
            key = ProcessCacheKey(key, isAppendAppCodeToKey);
            byte[] bytes = null;

            try
            {
                bytes = await _cache.GetAsync(key);
            }
            catch (Exception ex)
            {
                //_logService.LogError(ex, ex.Message);
            }

            if (bytes != null && bytes.Length > 0)
            {
                try
                {
                    var jsonValue = Encoding.UTF8.GetString(bytes);
                    return Converter.Deserialize<T>(jsonValue);
                }
                catch (Exception e)
                {
                    return bytes.ToObject<T>();
                }
            }

            return default(T);

        }

        private string ProcessCacheKey(string key, bool isAppendAppCodeToKey)
        {
            if (isAppendAppCodeToKey)
            {
                var applicationCode = _configService.GetAppSetting(AppSettingsKey.ApplicationCode);
                key = $"{(applicationCode == null ? "" : applicationCode)}_{key}";
            }
#if DEBUG
            key = $"debug_{key}";
#endif
            return key;
        }
    }
}