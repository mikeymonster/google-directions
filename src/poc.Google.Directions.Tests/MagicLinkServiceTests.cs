using FluentAssertions;
using poc.Google.Directions.Services;
using poc.Google.Directions.Tests.Builders;
using Wild.TestHelpers.Extensions;
using Xunit;

namespace poc.Google.Directions.Tests
{
    public class MagicLinkServiceTests
    {
        [Fact]
        public void MagicLinkService_Constructor_Guards_Against_Null_Parameters()
        {
            typeof(MagicLinkService).ShouldNotAcceptNullOrBadConstructorArguments();
        }

        [Fact]
        public void MagicLinkService_Returns_Correct_Link()
        {
            var service = new MagicLinkServiceBuilder().Build();

            var result = service.CreateDirectionsLink();

            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith("https://www.google.com/maps/dir/?api=1&");
        }
    }
}
