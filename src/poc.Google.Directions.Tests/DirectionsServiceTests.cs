using System;
using System.Threading.Tasks;
using FluentAssertions;
using poc.Google.Directions.Models;
using poc.Google.Directions.Services;
using poc.Google.Directions.Tests.Builders;
using Wild.TestHelpers.Extensions;
using Xunit;

namespace poc.Google.Directions.Tests
{
    public class DirectionsServiceTests
    {
        private const string TestApiKey = "test_key";

        private Location TestFromLocation => new Location
        {
            Postcode = "CV1 2WT",
            Longitude = -1.508122,
            Latitude = 52.400997
        };

        private Location TestToLocation => new Location
        {
            Postcode = "B91 1SB",
            Longitude = -1.792148,
            Latitude = 52.409568
        };

        [Fact]
        public void DirectionsService_Constructor_Guards_Against_Null_Parameters()
        {
            typeof(DirectionsService).ShouldNotAcceptNullOrBadConstructorArguments();
        }

        [Fact]
        public async void DirectionsService_Throws_Exception_For_Bad_Response()
        {
            var queryUrl = "https://bad.url.googleapis.com/";

            var service = new DirectionsServiceBuilder(
                    queryUrl,
                    TestApiKey,
                    new DirectionsJsonBuilder())
                .Build();

            Func<Task> sutMethod = async () => { await service.GetDirections(TestFromLocation, TestToLocation); };
            await sutMethod.Should().ThrowAsync<Exception>();

            //converter.Invoking(c =>
            //        c.ConvertBack("Anything", Arg.Any<Type>(), Arg.Any<object>(), Arg.Any<CultureInfo>()))
            //    .Should().Throw<NotImplementedException>();
        }

        [Fact]
        public async void DirectionsService_Returns_Expected_Journey()
        {
            // ReSharper disable once StringLiteralTypo
            const string queryUrl =
                "https://maps.googleapis.com/maps/api/directions/json?origin=52.400997,-1.508122&destination=52.409568,-1.792148&region=uk&mode=transit&transit_mode=bus|train";
            var service = new DirectionsServiceBuilder(
                    queryUrl,
                    TestApiKey,
                    new DirectionsJsonBuilder())
                .Build();

            var result = await service.GetDirections(TestFromLocation, TestToLocation);

            result.Should().NotBeNull();

        }

        [Fact]
        public async void DirectionsService_BuildJourneyFromJsonString_Returns_Expected_Journey()
        {
            var service = new DirectionsServiceBuilder().Build();


            var json = new DirectionsJsonBuilder().Build();

            var journey = await service.BuildJourneyFromJson(json);

            journey.Should().NotBeNull();

            journey.RawJson.Should().Be(json);

        }
    }
}
