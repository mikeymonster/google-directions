using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using poc.Google.Directions.Models;
using poc.Google.Directions.Services;
using poc.Google.Directions.Tests.Builders;
using Wild.TestHelpers.Extensions;
using Xunit;

namespace poc.Google.Directions.Tests
{
    public class PlacesServiceTests
    {
        private const string TestApiKey = "test_key";
        
        [Fact]
        public void PlacesService_Constructor_Guards_Against_Null_Parameters()
        {
            typeof(PlacesService).ShouldNotAcceptNullOrBadConstructorArguments();
        }

        [Fact]
        public async void PlacesService_Throws_Exception_For_Bad_Response()
        {
            var queryUrl = "https://bad.url.googleapis.com/";

            var service = new PlacesServiceBuilder(
                    queryUrl,
                    TestApiKey,
                    new PlacesJsonBuilder())
                .Build();

            Func<Task> sutMethod = async () => { await service.GetPlaces(LocationBuilder.FromLocation); };
            await sutMethod.Should().ThrowAsync<Exception>();
        }

        
        [Fact]
        public async void PlacesService_Returns_Expected_PlaceList()
        {
            // ReSharper disable once StringLiteralTypo
            const string queryUrl =
                "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=52.400997,-1.508122&rankby=distance&type=train_station%7Cbus_station%7Ctransit_station";

            var service = new PlacesServiceBuilder(
                    queryUrl,
                    TestApiKey,
                    new PlacesJsonBuilder())
                .Build();

            var result = await service.GetPlaces(LocationBuilder.FromLocation);

            result.Should().NotBeNull();
        }

        [Fact]
        public async void PlacesService_BuildPlacesFromJson_Returns_Raw_Json()
        {
            var service = new PlacesServiceBuilder().Build();

            var journey = await service.BuildPlacesFromJson(await new PlacesJsonBuilder().BuildStream());

            journey.Should().NotBeNull();
            //TODO: Fix the json or see why it isn't matching
            //journey.RawJson.Should().Be(json);
        }

        [Fact]
        public async void PlacesService_BuildPlacesFromJson_Returns_Expected_Places()
        {
            var service = new PlacesServiceBuilder().Build();
            var places = await service.BuildPlacesFromJson(await new PlacesJsonBuilder().BuildStream());

            places.Should().NotBeNull();

            places.PlaceList.Should().NotBeNullOrEmpty();
            places.PlaceList.Count.Should().Be(10);

            /*
            journey.Routes.Should().NotBeNullOrEmpty();
            journey.Routes.Count.Should().Be(1);

            var route = journey.Routes.First();
            route.Summary.Should().Be("Route summary");
            route.Warnings.Should().NotBeNullOrEmpty();
            route.Warnings.Count.Should().Be(1);
            route.Warnings.First().Should().StartWith("Walking directions are in beta. Use caution ");
            route.Warnings.First().Should().EndWith(" This route may be missing sidewalks or pedestrian paths.");
            route.Warnings.First().Should().Be("Walking directions are in beta. Use caution - This route may be missing sidewalks or pedestrian paths.");
        */

        }
    }
}
