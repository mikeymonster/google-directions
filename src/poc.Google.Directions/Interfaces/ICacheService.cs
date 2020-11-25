using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
// ReSharper disable UnusedMemberInSuper.Global

namespace poc.Google.Directions.Interfaces
{
    public interface ICacheService
    {
        TItem Get<TItem>(string key);

        void Set<TItem>(string key, TItem value, TimeSpan expiry);

        Task<TItem> GetOrCreate<TItem>(string key, Func<ICacheEntry, Task<TItem>> factory);
    }
}
