using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Core.Interface
{
    public interface ICacheService
    {
        /// <summary>
        /// Thêm object vào cache
        /// </summary>
        /// <param name="key">cache key</param>
        /// <param name="value">object cần cache</param>
        /// <param name="isAbsoluteExpiration"></param>
        /// <param name="isAppendAppCodeToKey"></param>
        /// <returns></returns>
        Task Set(string key, object value, bool isAbsoluteExpiration = false, bool isAppendAppCodeToKey = true);
        
        /// <summary>
        /// Thêm object vào cache, có thời gian hết hạn
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeout">thời gian hết hạn</param>
        /// <param name="isAbsoluteExpiration"></param>
        /// <param name="isAppendAppCodeToKey"></param>
        /// <returns></returns>
        Task Set(string key, object value,TimeSpan timeout, bool isAbsoluteExpiration = false, bool isAppendAppCodeToKey = true);

        /// <summary>
        /// Xoá giá trị trong cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isAppendAppCodeToKey"></param>
        /// <returns></returns>
        Task Delete(string key, bool isAppendAppCodeToKey = true);
        
        /// <summary>
        /// lấy object trong cache
        /// </summary>
        /// <param name="key">cache key</param>
        /// <param name="isAppendAppCodeToKey"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Get<T>(string key, bool isAppendAppCodeToKey = true);
        
        
    }
    public interface IMemoryCacheService: ICacheService
    {
            
    }
    public interface IMemoryDistributedCache : IDistributedCache
    {
            
    }
}