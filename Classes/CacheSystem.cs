using System;
using System.Threading.Tasks;
using LazyCache;

namespace BetaManager.Classes
{
    internal class CacheSystem
    {
        private static CachingService Cache = new CachingService(
            CachingService.DefaultCacheProvider
        );

        public static async Task<T> GetOrAdd<T>(string key, Func<Task<T>> func)
        {
            T Cached = await Cache.GetAsync<T>(key);
            if (Cached == null)
            {
                return await UpdateOrAdd(key, await func());
            }
            else
                return Cached;
        }

        public static async Task<T> Get<T>(string key)
        {
            return await Cache.GetAsync<T>(key);
        }

        public static async void Delete(string cacheKey)
        {
            Cache.Remove(cacheKey);
        }

        public static async Task<T> UpdateOrAdd<T>(string key, T data)
        {
            Cache.Remove(key);
            Cache.Add(key, data);
            return await Get<T>(key);
        }
    }
}
