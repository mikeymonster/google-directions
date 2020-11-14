using FluentAssertions;
using poc.Google.Directions.Tests.Builders;
using Xunit;

namespace poc.Google.Directions.Tests
{
    public class MagicLinkServiceTests
    {
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
