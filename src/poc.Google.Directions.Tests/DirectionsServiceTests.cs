using FluentAssertions;
using poc.Google.Directions.Models;
using poc.Google.Directions.Services;
using poc.Google.Directions.Tests.Builders;
using Xunit;

namespace poc.Google.Directions.Tests
{
    public class DirectionsServiceTests
    {
        [Fact]
        public void DirectionsService_Returns_Correct_Directions()
        {
            var service = new DirectionsServiceBuilder().Build();

            var result = service.GetDirections();

            result.Should().NotBeNull();
        }
    }
}
