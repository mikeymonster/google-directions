using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using poc.Google.Directions.Services;
using poc.Google.Directions.Tests.Builders;
using Wild.TestHelpers.Extensions;
using Xunit;

namespace poc.Google.Directions.Tests
{
    public class DirectionsServiceTests
    {
        private const string TestApiKey = "test_key";
        
        [Fact]
        public void DirectionsService_Constructor_Guards_Against_Null_Parameters()
        {
            typeof(DirectionsService).ShouldNotAcceptNullOrBadConstructorArguments();
        }

        [Fact]
        public async void DirectionsService_Throws_Exception_For_Bad_Response()
        {
            const string queryUrl = "https://bad.url.googleapis.com/";

            var service = new DirectionsServiceBuilder(
                    queryUrl,
                    TestApiKey,
                    new DirectionsJsonBuilder())
                .Build();

            Func<Task> sutMethod = async () => { await service.GetDirections(LocationBuilder.FromLocation, LocationBuilder.ToLocation); };
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
                "https://maps.googleapis.com/maps/api/directions/json?origin=52.400997,-1.508122&destination=52.409568,-1.792148&region=uk&mode=transit&transit_mode=train|bus";
            var service = new DirectionsServiceBuilder(
                    queryUrl,
                    TestApiKey,
                    new DirectionsJsonBuilder())
                .Build();

            var result = await service.GetDirections(LocationBuilder.FromLocation, LocationBuilder.ToLocation);

            result.Should().NotBeNull();
        }

        [Fact]
        public async void DirectionsService_BuildJourneyFromJsonString_Returns_Raw_Json()
        {
            var service = new DirectionsServiceBuilder().Build();

            var journey = await service.BuildJourneyFromJson(await new DirectionsJsonBuilder().BuildStream());

            journey.Should().NotBeNull();
            //Not 
            //journey.RawJson.Should().Be(json);
        }

        [Fact]
        public async void DirectionsService_BuildJourneyFromJsonString_Returns_Expected_Journey()
        {
            var service = new DirectionsServiceBuilder().Build();
            var journey = await service.BuildJourneyFromJson(await new DirectionsJsonBuilder().BuildStream());

            journey.Should().NotBeNull();

            journey.Routes.Should().NotBeNullOrEmpty();
            journey.Routes.Count.Should().Be(1);

            var route = journey.Routes.First();
            route.Summary.Should().Be("Route summary");
            route.Warnings.Should().NotBeNullOrEmpty();
            route.Warnings.Count.Should().Be(1);
            route.Warnings.First().Should().StartWith("Walking directions are in beta. Use caution ");
            route.Warnings.First().Should().EndWith(" This route may be missing sidewalks or pedestrian paths.");
            route.Warnings.First().Should().Be("Walking directions are in beta. Use caution - This route may be missing sidewalks or pedestrian paths.");

            route.Legs.Should().NotBeNullOrEmpty();
            route.Legs.Count.Should().Be(1);

            var leg = route.Legs.First();
            leg.StartAddress.Should().Be("Cheylesmore House, 5 Quinton Rd, Coventry CV1 2WT, UK");
            leg.EndAddress.Should().Be("Halls of Residence, Blossomfield Rd, Solihull B91 1SZ, UK");
            leg.Distance.Should().Be(28541);
            leg.DistanceString.Should().Be("28.5 km");
            leg.Duration.Should().Be(4138);
            leg.DurationString.Should().Be("1 hour 9 mins");

            leg.Steps.Should().NotBeNullOrEmpty();
            //leg.Steps.Count.Should().Be(1);

            var step = leg.Steps.First();

            step.Instructions.Should().Be("Walk to Coventry");   
            step.TravelMode.Should().Be("WALKING");
            
            step.Distance.Should().Be(780);
            step.DistanceString.Should().Be("0.8 km");
            step.Duration.Should().Be(584);
            step.DurationString.Should().Be("10 mins");

            step.StartLatitude.Should().Be(52.40135360000001);
            step.StartLongitude.Should().Be(-1.508031);

            step.EndLatitude.Should().Be(52.40082899999999);
            step.EndLongitude.Should().Be(-1.51345);

            //"start_location": {
            //    "lat": 52.40135360000001,
            //    "lng": -1.508031
            //},
            //"end_location": {
            //    "lat": 52.40082899999999,
            //    "lng": -1.51345
            //},

            step.Steps.Should().NotBeNull();
            //step.Steps.Should().NotBeNullOrEmpty();
            //step.Steps.Count.Should().Be(1);

            step.TransitDetails.Should().NotBeNull();

            //TODO: Verify transit details

        }
    }
}
