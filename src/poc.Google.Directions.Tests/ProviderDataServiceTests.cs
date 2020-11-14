using FluentAssertions;
using poc.Google.Directions.Tests.Builders;
using Xunit;

namespace poc.Google.Directions.Tests
{
    public class ProviderDataServiceTests
    {
        [Fact]
        public void ProviderDataService_Get_Home_Returns_Expected_Result()
        {
            var service = new ProviderDataServiceBuilder().Build();

            var result = service.GetHome();

            result.Should().NotBeNull();
        }

        [Fact]
        public void ProviderDataService_GetProviders_Returns_Expected_Result()
        {
            var service = new ProviderDataServiceBuilder().Build();

            var result = service.GetProviders();

            result.Should().NotBeNullOrEmpty();
        }

    }
}
