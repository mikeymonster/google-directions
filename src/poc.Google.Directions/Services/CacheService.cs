using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using poc.Google.Directions.Interfaces;

namespace poc.Google.Directions.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache)); ;
        }

        public TItem Get<TItem>(string key) 
        {
            return _cache.TryGetValue(key, out TItem value)
                ? value
                : default;
        }

        public Task<TItem> GetOrCreate<TItem>(string key, Func<ICacheEntry, Task<TItem>> factory)
        {
            return _cache.GetOrCreateAsync(key, factory);
        }

        public void Set<TItem>(string key, TItem value, TimeSpan expiry)
        {
            _cache.Set(key, value,
                new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(expiry));
        }
    }
}
