using System.Threading.Tasks;
using FluentAssertions;
using poc.Google.Directions.Services;
using poc.Google.Directions.Tests.Builders;
using Wild.TestHelpers.Extensions;
using Xunit;

namespace poc.Google.Directions.Tests
{
    public class ProviderDataServiceTests
    {
        [Fact]
        public void ProviderDataService_Constructor_Guards_Against_Null_Parameters()
        {
            typeof(ProviderDataService).ShouldNotAcceptNullOrBadConstructorArguments();
        }

        [Fact]
        public async Task ProviderDataService_Get_Home_Returns_Expected_Result()
        {
            var service = new ProviderDataServiceBuilder().Build();

            var result = await service.GetHome();

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ProviderDataService_GetProviders_Returns_Expected_Result()
        {
            var service = new ProviderDataServiceBuilder().Build();

            var result = await service.GetProviders();

            result.Should().NotBeNullOrEmpty();
        }
    }
}
