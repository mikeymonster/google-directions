using System.Net.Http;
using NSubstitute;
using poc.Google.Directions.Models;
using poc.Google.Directions.Services;

namespace poc.Google.Directions.Tests.Builders
{
    public class DirectionsServiceBuilder
    {
        private readonly DirectionsService _directionsService;

        public DirectionsServiceBuilder(
            ApiSettings settings = null,
            IHttpClientFactory httpClientFactory = null)
        {
            settings ??= new ApiSettings
            {
                GoogleApiKey = "test_key"
            };

            httpClientFactory ??= Substitute.For<IHttpClientFactory>();
            
            _directionsService = new DirectionsService(settings, httpClientFactory);
        }

        public DirectionsService Build() => _directionsService;
    }
}
