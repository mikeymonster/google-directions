using System;
using System.Net.Http;
using NSubstitute;
using poc.Google.Directions.Models;
using poc.Google.Directions.Services;
using Wild.TestHelpers.HttpClient;

namespace poc.Google.Directions.Tests.Builders
{
    public class PlacesServiceBuilder
    {
        private readonly PlacesService _placesService;

        public ApiSettings CreateApiSettings(string apiKey = "test_key") 
            => new ApiSettings
            {
                GooglePlacesApiKey = apiKey
            };

        public PlacesServiceBuilder(
            IHttpClientFactory httpClientFactory = null,
            ApiSettings settings = null)
        {
            settings ??= CreateApiSettings();

            httpClientFactory ??= Substitute.For<IHttpClientFactory>();
            
            _placesService = new PlacesService(settings, httpClientFactory);
        }

        public PlacesServiceBuilder(
            string queryUri,
            string apiKey,
            PlacesJsonBuilder dataBuilder,
            ApiSettings settings = null)
        {
            settings ??= CreateApiSettings(apiKey);

            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory
                .CreateClient(nameof(PlacesService))
                .Returns(new TestHttpClientFactory()
                    .CreateHttpClient(new Uri($"{queryUri}&key={apiKey}"),
                        dataBuilder.Build()));

            _placesService = new PlacesService(settings, httpClientFactory);
        }

        public PlacesService Build() => _placesService;
    }
}
