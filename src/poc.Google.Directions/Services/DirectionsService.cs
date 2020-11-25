using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using poc.Google.Directions.Extensions;
using poc.Google.Directions.Interfaces;
using poc.Google.Directions.Models;

namespace poc.Google.Directions.Services
{
    public class DirectionsService : IDirectionsService
    {
        private readonly ApiSettings _settings;

        private const string BaseUrl = "https://maps.googleapis.com/maps/api/directions/json?";

        private readonly IHttpClientFactory _httpClientFactory;

        public DirectionsService(ApiSettings settings, IHttpClientFactory httpClientFactory)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public Models.Directions GetDirections()
        {
            return new Models.Directions();
        }

        public async Task<Journey> GetDirections(Location from, Location to)
        {
            if (string.IsNullOrWhiteSpace(_settings.GoogleApiKey)) return null;

            var httpClient = _httpClientFactory.CreateClient(nameof(DirectionsService));
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            //httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            //Build uri to google directions...

            //Google API call here: https://developers.google.com/maps/documentation/directions/overview

            //Consider:
            //https://stackoverflow.com/questions/829080/how-to-build-a-query-string-for-a-url-in-c/10836145#10836145
            //QueryHelpers.AddQueryString()

            var uriBuilder = new StringBuilder(BaseUrl);

            uriBuilder.Append($"origin={from.Latitude},{from.Longitude}");
            uriBuilder.Append($"&destination={to.Latitude},{to.Longitude}");
            uriBuilder.Append("&region=uk");
            uriBuilder.Append("&mode=transit");
            uriBuilder.Append("&transit_mode=bus|train");
            uriBuilder.Append($"&key={_settings.GoogleApiKey}");
            
            var uri = new Uri(uriBuilder.ToString());

            var responseMessage = await httpClient.GetAsync(uri);

            Debug.WriteLine($"Google response code {responseMessage.StatusCode}. Reason {responseMessage.ReasonPhrase}");

            responseMessage.EnsureSuccessStatusCode();

            var content = await responseMessage.Content.ReadAsStringAsync();

            Debug.WriteLine($"Google response: {Environment.NewLine}{content.PrettifyJsonString()}");

            return new Journey
            {
                RawJson = content,
                Distance = 0,
                DistanceFromNearestBusStop = 0,
                DistanceFromNearestTrainStop = 0,
                Steps = new List<string>()
            };
        }
    }
}
