using System;
using System.Net.Http;
using NSubstitute;
using poc.Google.Directions.Models;
using poc.Google.Directions.Services;
using Wild.TestHelpers.HttpClient;

namespace poc.Google.Directions.Tests.Builders
{
    public class DirectionsServiceBuilder
    {
        private readonly DirectionsService _directionsService;

        public ApiSettings CreateApiSettings(string apiKey = "test_key") 
            => new ApiSettings
            {
                GoogleApiKey = apiKey

            };

        public DirectionsServiceBuilder(
            IHttpClientFactory httpClientFactory = null,
            ApiSettings settings = null)
        {
            settings ??= CreateApiSettings();

            httpClientFactory ??= Substitute.For<IHttpClientFactory>();
            
            _directionsService = new DirectionsService(settings, httpClientFactory);
        }

        public DirectionsServiceBuilder(
            string queryUri,
            string apiKey,
            DirectionsJsonBuilder dataBuilder,
            ApiSettings settings = null)
        {
            settings ??= CreateApiSettings(apiKey);

            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory
                .CreateClient(nameof(DirectionsService))
                .Returns(new TestHttpClientFactory()
                    .CreateHttpClient(new Uri($"{queryUri}&key={apiKey}"),
                        dataBuilder.Build()));

            _directionsService = new DirectionsService(settings, httpClientFactory);
        }

        public DirectionsService Build() => _directionsService;
    }
}
