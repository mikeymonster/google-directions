using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using poc.Google.Directions.Extensions;
using poc.Google.Directions.Interfaces;
using poc.Google.Directions.Models;

namespace poc.Google.Directions.Services
{
    public class PlacesService : IPlacesService
    {
        private readonly ApiSettings _settings;

        private const string BaseUrl = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?";
        private readonly IHttpClientFactory _httpClientFactory;

        public PlacesService(ApiSettings settings, IHttpClientFactory httpClientFactory)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<Places> GetPlaces(Location nearTo, bool useTrainTransitMode = true, bool useBusTransitMode = true, int searchRadiusInMeters = 5000, bool rankByDistance = true)
        {
            if (string.IsNullOrWhiteSpace(_settings.GooglePlacesApiKey)) return null;

            var httpClient = _httpClientFactory.CreateClient(nameof(PlacesService));
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            //TODO: Build uri and set correct base uri above
            /*
                https://developers.google.com/places/supported_types 

                https://stackoverflow.com/questions/10078755/google-places-api-subway-station

                https://jdunkerley.co.uk/2016/07/21/geocoding-and-finding-nearest-station-with-google-web-services/

                https://developers.google.com/places/web-service/usage-and-billing

                https://developers.google.com/places/web-service/search#PlaceSearchRequests
             */

            var uriBuilder = new StringBuilder(BaseUrl);

            uriBuilder.Append($"location={nearTo.Latitude},{nearTo.Longitude}");
            //uriBuilder.Append("&region=uk");
            //uriBuilder.Append("&mode=transit");
            if (rankByDistance)
            {
                uriBuilder.Append("&rankby=distance");  // must not be included if radius is specified
            }
            else
            {
                uriBuilder.Append($"&radius={searchRadiusInMeters}");  // must not be included if rankby=distance is specified
            }

            //Only one type is allowed
            var typeBuilder = new StringBuilder();
            if (useTrainTransitMode) typeBuilder.Append("train_station");
            if (useBusTransitMode)
            {
                if (typeBuilder.Length > 0) typeBuilder.Append("|");
                typeBuilder.Append("bus_station");
                typeBuilder.Append("|transit_station");
            }

            if (typeBuilder.Length > 0)
            {
                uriBuilder.Append($"&type={typeBuilder}");
            }

            uriBuilder.Append($"&key={_settings.GooglePlacesApiKey}");

            var uri = new Uri(uriBuilder.ToString());

            //This works:
            //https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=-33.8670522,151.1957362&radius=1500&type=restaurant&keyword=cruise&key=

            var responseMessage = await httpClient.GetAsync(uri);

            Debug.WriteLine(
                $"Google response code {responseMessage.StatusCode}. Reason {responseMessage.ReasonPhrase}");

            responseMessage.EnsureSuccessStatusCode();

            var content = await responseMessage.Content.ReadAsStringAsync();

            Debug.WriteLine($"Google response: {Environment.NewLine}{content.PrettifyJson()}");

            return await BuildPlacesFromJson(await responseMessage.Content.ReadAsStreamAsync());
        }

        public async Task<Places> BuildPlacesFromJson(Stream jsonStream)
        {
            var jsonDoc = await JsonDocument.ParseAsync(jsonStream);

            var places = new Places
            {
                RawJson = jsonDoc.PrettifyJson(),
                PlaceList = new List<Place>()
            };

            foreach (var resultElement in jsonDoc.RootElement.GetProperty("results").EnumerateArray())
            {
                JsonElement locationElement = default;

                var hasLocation = resultElement.TryGetProperty("geometry", out var geometryElement) &&
                                  geometryElement.TryGetProperty("location", out locationElement);

                places.PlaceList.Add(new Place
                {
                    Name = resultElement.SafeGetString("name"),

                    Latitude = hasLocation ? locationElement.SafeGetDouble("lat") : default,
                    Longitude = hasLocation ? locationElement.SafeGetDouble("lng") : default,

                    Types = resultElement.TryGetProperty("types", out var types)
                        ? new List<string>(types
                            .EnumerateArray()
                            .Select(t => t.GetString())
                            .ToList())
                        : new List<string>()
                });
            }

            return places;
            }
        }
    }
