using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
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
            //NOTE: searched for B91 1SB but the destination in the results is B91 1SZ
            //TODO: Would be faster to do this from the content stream - look at this after test is working
            //https://stu.dev/a-look-at-jsondocument/
            //var jsonDoc = await JsonDocument.ParseAsync(await responseMessage.Content.ReadAsStreamAsync());

            //var options = new JsonSerializerOptions
            //{
            //    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //    WriteIndented = true
            //};

            //JsonDocument jsonDoc;
            //if (jsonStream != null)
            var jsonDoc = await JsonDocument.ParseAsync(jsonStream);
            //else 
            //var jsonDoc = JsonDocument.Parse(json);//, options);

            //var root = jsonDoc.RootElement;

            var places = new Places
            {
                RawJson = jsonDoc.PrettifyJson(),
            };

            //var routes = root.GetProperty("routes");
            //foreach (var routeElement in jsonDoc.RootElement.GetProperty("routes").EnumerateArray())
            //{
            //    //Debug.WriteLine($"Route {routeElement.Name}");

            //    var route = new Route
            //    {
            //        Summary = routeElement.SafeGetString("summary"),
            //        Warnings =
            //            routeElement.TryGetProperty("warnings", out var warnings)
            //            ? new List<string>(
            //                routeElement.GetProperty("warnings")
            //                        .EnumerateArray()
            //                .Select(w =>
            //                {
            //                    //TODO: Sort out loading of incorrect characters here 
            //                    Debug.WriteLine(w.GetString());
            //                    Debug.WriteLine((int)w.GetString()[44]);
            //                    Debug.WriteLine($"{(int)w.GetString()[44]:X}");
            //                    return w.GetString();
            //                })
            //                .ToList())
            //        : new List<string>(),
            //        Legs = new List<Leg>()
            //    };
            //    journey.Routes.Add(route);

            //    foreach (var legElement in routeElement.GetProperty("legs").EnumerateArray())
            //    {
            //        var hasLegDistance = legElement.TryGetProperty("distance", out var legDistanceElement);
            //        var hasLegDuration = legElement.TryGetProperty("duration", out var legDurationElement);

            //        var leg = new Leg
            //        {
            //            StartAddress = legElement.SafeGetString("start_address"),
            //            EndAddress = legElement.SafeGetString("end_address"),
            //            Distance = hasLegDistance ? legDistanceElement.SafeGetInt32("value") : default,
            //            DistanceString = hasLegDistance ? legDistanceElement.SafeGetString("text") : default,
            //            Duration = hasLegDuration ? legDurationElement.SafeGetInt32("value") : default,
            //            DurationString = hasLegDuration ? legDurationElement.SafeGetString("text") : default,
            //            Steps = new List<Step>()
            //        };
            //        route.Legs.Add(leg);

            //        foreach (var stepElement in legElement.GetProperty("steps").EnumerateArray())
            //        {
            //            var hasStepDistanceElement = stepElement.TryGetProperty("distance", out var stepDistanceElement);
            //            var hasStepDurationElement = stepElement.TryGetProperty("duration", out var stepDurationElement);

            //            var hasStepStartLocation = stepElement.TryGetProperty("start_location", out var stepStartLocationElement);
            //            var hasStepEndLocation = stepElement.TryGetProperty("end_location", out var stepEndLocationElement);

            //            var step = new Step
            //            {
            //                Distance = hasStepDistanceElement ? stepDistanceElement.SafeGetInt32("value") : default,
            //                DistanceString = hasStepDistanceElement ? stepDistanceElement.SafeGetString("text") : default,
            //                Duration = hasStepDistanceElement ? stepDurationElement.SafeGetInt32("value") : default,
            //                DurationString = hasStepDistanceElement ? stepDurationElement.SafeGetString("text") : default,

            //                //StartLatitude = stepElement.GetProperty("start_location").GetProperty("lat").GetDouble(),
            //                //StartLongitude = stepElement.GetProperty("start_location").GetProperty("lng").GetDouble(),

            //                StartLatitude = hasStepStartLocation ? stepStartLocationElement.SafeGetDouble("lat") : default,
            //                StartLongitude = hasStepStartLocation ? stepStartLocationElement.SafeGetDouble("lng") : default,

            //                EndLatitude = hasStepEndLocation ? stepEndLocationElement.SafeGetDouble("lat") : default,
            //                EndLongitude = hasStepEndLocation ? stepEndLocationElement.SafeGetDouble("lng") : default,
            //                //EndLatitude = stepElement.GetProperty("end_location").GetProperty("lat").GetDouble(),
            //                //EndLongitude = stepElement.GetProperty("end_location").GetProperty("lng").GetDouble(),

            //                Instructions = stepElement.SafeGetString("html_instructions"),
            //                TravelMode = stepElement.SafeGetString("travel_mode"),

            //                Steps = new List<Step>()
            //            };
            //            leg.Steps.Add(step);

            //            if (stepElement.TryGetProperty("steps", out var innerStepsArray))
            //            {
            //                foreach (var innerStepElement in innerStepsArray.EnumerateArray())
            //                {
            //                    step.Steps.Add(new Step
            //                    {
            //                        Distance = innerStepElement.GetProperty("distance").GetProperty("value").GetInt32(),
            //                        DistanceString = innerStepElement.GetProperty("distance").GetProperty("text")
            //                            .GetString(),
            //                        Duration = innerStepElement.GetProperty("duration").GetProperty("value").GetInt32(),
            //                        DurationString = innerStepElement.GetProperty("duration").GetProperty("text")
            //                            .GetString(),

            //                        StartLatitude = innerStepElement.GetProperty("start_location").GetProperty("lat")
            //                            .GetDouble(),
            //                        StartLongitude = innerStepElement.GetProperty("start_location").GetProperty("lng")
            //                            .GetDouble(),

            //                        EndLatitude = innerStepElement.GetProperty("end_location").GetProperty("lat")
            //                            .GetDouble(),
            //                        EndLongitude = innerStepElement.GetProperty("end_location").GetProperty("lng")
            //                            .GetDouble(),

            //                        Instructions = innerStepElement.SafeGetString("html_instructions"),

            //                        Steps = new List<Step>()
            //                    });

            //                    if (innerStepElement.TryGetProperty("transit_details",
            //                        out var innerTransitDetailsProperty))
            //                    {

            //                    }

            //                }
            //            }

            //            step.TransitDetails = new TransitDetails();

            //            //TODO: Look at transit_details
            //            if (stepElement.TryGetProperty("transit_details", out var transitDetailsProperty))
            //            {
            //                if (transitDetailsProperty.TryGetProperty("line", out var line))
            //                {
            //                    //var lineName = line.TryGetProperty("line", out var lineName)
            //                }

            //                //var line =

            //            //This is messy - need to break down the individual pieces
            //            //TODO: Add a helper that gets string using Try then returns GetString or null
            //            //      Same for double

            //            //step.TransitDetails = new TransitDetails
            //            //{
            //                //ArrivalStopName
            //                //ArrivalStopLatitude
            //                //ArrivalStopLongitude
            //                //DepartureStopName
            //                //DepartureStopLatitude
            //                //DepartureStopLongitude
            //                //    LineName = transitDetailsProperty.GetProperty("line_name").GetProperty("name")
            //                //LineShortName
            //                //LineVehicleName
            //                //LineVehicleType
            //                //NumStops =
            //            //};
            //            }
            //        }
            //    }
            //}

            return places;
        }
    }
}
