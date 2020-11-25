using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using poc.Google.Directions.Services;
using Wild.TestHelpers.Extensions;
using Xunit;

namespace poc.Google.Directions.Tests
{
    public class CacheServiceTests
    {
        private const string CacheKey = "Test_Cache_Key";

        [Fact]
        public void CacheService_Constructor_Guards_Against_Null_Parameters()
        {
            typeof(CacheService).ShouldNotAcceptNullOrBadConstructorArguments();
        }

        [Fact]
        public void CacheService_GetOrCreate_Returns_Expected_Object_From_The_Cache()
        {
            var cachedObject = new object();
            var cache = SetupMemoryCache(cachedObject);

            var service = new CacheService(cache);

            var returnedObject = service.GetOrCreate(CacheKey,
                entry => Task.FromResult(cachedObject)).GetAwaiter().GetResult();
            returnedObject.Should().Be(cachedObject);
        }

        [Fact]
        public void CacheService_GetOrCreate_Calls_TryGetValue_Exactly_Once()
        {
            var cachedObject = new object();
            var cache = SetupMemoryCache(cachedObject);

            var service = new CacheService(cache);

            service.GetOrCreate(CacheKey,
                entry => Task.FromResult(cachedObject)).GetAwaiter().GetResult();

            cache.Received(1).TryGetValue(CacheKey, out Arg.Any<object>());
        }

        [Fact]
        public void CacheService_GetOrCreate_Calls_Cache_GetOrCreateAsync_Exactly_Once()
        {
            var cachedObject = new object();
            var cache = SetupMemoryCache(cachedObject);

            var service = new CacheService(cache);

            service.GetOrCreate(CacheKey,
                entry => Task.FromResult(cachedObject)).GetAwaiter().GetResult();

            cache.Received(1)
                .GetOrCreateAsync(CacheKey, Arg.Any<Func<ICacheEntry, Task<object>>>());
        }

        [Fact]
        public void CacheService_Set_Then_Get_Calls_Cache_Set_Exactly_Once()
        {
            var cachedObject = new object();
            var expiry = TimeSpan.MaxValue;
            var cache = SetupMemoryCache(cachedObject, expiry);

            var service = new CacheService(cache);

            service.Set(CacheKey, cachedObject, expiry);
            service.Get<object>(CacheKey);
            //var returnedObject = service.Get<object>(CacheKey);

            cache.Received().Set(CacheKey, cachedObject, expiry);
        }

        [Fact]
        public void CacheService_Set_Then_Get_Returns_Expected_Object()
        {
            var cachedObject = new object();
            var expiry = TimeSpan.MaxValue;
            var cache = SetupMemoryCache(cachedObject, expiry);

            var service = new CacheService(cache);

            service.Set(CacheKey, cachedObject, expiry);
            var returnedObject = service.Get<object>(CacheKey);

            returnedObject.Should().Be(cachedObject);
        }

        [Fact]
        public void CacheService_Set_Then_Get_Calls_Cache_TryGetValue_Exactly_Once()
        {
            var cachedObject = new object();
            var expiry = TimeSpan.MaxValue;
            var cache = SetupMemoryCache(cachedObject, expiry);

            var service = new CacheService(cache);

            service.Set(CacheKey, cachedObject, expiry);
            service.Get<object>(CacheKey);

            cache.Received(1).TryGetValue(CacheKey, out Arg.Any<object>());
        }

        private static IMemoryCache SetupMemoryCache(object cachedObject, TimeSpan? expiry = null)
        {
            expiry ??= TimeSpan.MaxValue;

            var cache = Substitute.For<IMemoryCache>();

            cache.Set(CacheKey, cachedObject, expiry.Value)
                .Returns(x => x[1]);

            cache.TryGetValue<object>(CacheKey, out Arg.Any<object>())
                .Returns(x =>
                {
                    x[1] = cachedObject;
                    return true;
                });

            return cache;
        }
    }
}