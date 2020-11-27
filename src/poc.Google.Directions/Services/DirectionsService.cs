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

        public async Task<Journey> GetDirections(Location from, Location to, bool useTrainTransitMode = true, bool useBusTransitMode = true)
        {
            if (string.IsNullOrWhiteSpace(_settings.GoogleApiKey)) return null;

            var httpClient = _httpClientFactory.CreateClient(nameof(DirectionsService));
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

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

            var transitModeBuilder = new StringBuilder();
            if (useTrainTransitMode) transitModeBuilder.Append("train");
            if (useBusTransitMode)
            {
                if (transitModeBuilder.Length > 0) transitModeBuilder.Append("|");
                transitModeBuilder.Append("bus");
            }

            if (transitModeBuilder.Length > 0)
            {
                uriBuilder.Append($"&transit_mode={transitModeBuilder}");
            }

            uriBuilder.Append($"&key={_settings.GoogleApiKey}");

            var uri = new Uri(uriBuilder.ToString());

            var responseMessage = await httpClient.GetAsync(uri);

            Debug.WriteLine(
                $"Google response code {responseMessage.StatusCode}. Reason {responseMessage.ReasonPhrase}");

            responseMessage.EnsureSuccessStatusCode();

            var content = await responseMessage.Content.ReadAsStringAsync();

            Debug.WriteLine($"Google response: {Environment.NewLine}{content.PrettifyJson()}");

            return await BuildJourneyFromJson(await responseMessage.Content.ReadAsStreamAsync());
        }

        public async Task<Journey> BuildJourneyFromJson(Stream jsonStream)
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

            var journey = new Journey
            {
                RawJson = jsonDoc.PrettifyJson(),
                Distance = 0,
                DistanceFromNearestBusStop = 0,
                DistanceFromNearestTrainStop = 0,
                Steps = new List<string>(),
                Routes = new List<Route>()
            };


            //var routes = root.GetProperty("routes");
            foreach (var routeElement in jsonDoc.RootElement.GetProperty("routes").EnumerateArray())
            {
                //Debug.WriteLine($"Route {routeElement.Name}");

                var route = new Route
                {
                    Summary = routeElement.GetProperty("summary").GetString(),
                    Warnings = new List<string>(
                        routeElement.GetProperty("warnings")
                        .EnumerateArray()
                        .Select(w =>
                        {
                            Debug.WriteLine(w.GetString());
                            Debug.WriteLine((int)w.GetString()[44]);
                            Debug.WriteLine($"{(int)w.GetString()[44]:X}");
                            return w.GetString();
                        })
                        .ToList()),
                    Legs = new List<Leg>()
                };
                journey.Routes.Add(route);

                foreach (var legElement in routeElement.GetProperty("legs").EnumerateArray())
                {
                    var leg = new Leg
                    {
                        StartAddress = legElement.GetProperty("start_address").GetString(),
                        EndAddress = legElement.GetProperty("end_address").GetString(),
                        Distance = legElement.GetProperty("distance").GetProperty("value").GetInt32(),
                        DistanceString = legElement.GetProperty("distance").GetProperty("text").GetString(),
                        Duration = legElement.GetProperty("duration").GetProperty("value").GetInt32(),
                        DurationString = legElement.GetProperty("duration").GetProperty("text").GetString(),
                        Steps = new List<Step>()
                    };
                    route.Legs.Add(leg);

                    foreach (var stepElement in legElement.GetProperty("steps").EnumerateArray())
                    {
                        var step = new Step
                        {
                            Distance = stepElement.GetProperty("distance").GetProperty("value").GetInt32(),
                            DistanceString = stepElement.GetProperty("distance").GetProperty("text").GetString(),
                            Duration = stepElement.GetProperty("duration").GetProperty("value").GetInt32(),
                            DurationString = stepElement.GetProperty("duration").GetProperty("text").GetString(),

                            StartLatitude = stepElement.GetProperty("start_location").GetProperty("lat").GetDouble(),
                            StartLongitude = stepElement.GetProperty("start_location").GetProperty("lng").GetDouble(),

                            EndLatitude = stepElement.GetProperty("end_location").GetProperty("lat").GetDouble(),
                            EndLongitude = stepElement.GetProperty("end_location").GetProperty("lng").GetDouble(),

                            Instructions = stepElement.GetProperty("html_instructions").GetString(),
                            TravelMode = stepElement.GetProperty("travel_mode").GetString(),

                            Steps = new List<Step>()
                        };
                        leg.Steps.Add(step);

                        if (stepElement.TryGetProperty("steps", out var innerStepsArray))
                        {
                            foreach (var innerStepElement in innerStepsArray.EnumerateArray())
                            {
                                step.Steps.Add(new Step
                                {
                                    Distance = innerStepElement.GetProperty("distance").GetProperty("value").GetInt32(),
                                    DistanceString = innerStepElement.GetProperty("distance").GetProperty("text")
                                        .GetString(),
                                    Duration = innerStepElement.GetProperty("duration").GetProperty("value").GetInt32(),
                                    DurationString = innerStepElement.GetProperty("duration").GetProperty("text")
                                        .GetString(),

                                    StartLatitude = innerStepElement.GetProperty("start_location").GetProperty("lat")
                                        .GetDouble(),
                                    StartLongitude = innerStepElement.GetProperty("start_location").GetProperty("lng")
                                        .GetDouble(),

                                    EndLatitude = innerStepElement.GetProperty("end_location").GetProperty("lat")
                                        .GetDouble(),
                                    EndLongitude = innerStepElement.GetProperty("end_location").GetProperty("lng")
                                        .GetDouble(),

                                    Instructions = innerStepElement.GetProperty("html_instructions").GetString(),

                                    Steps = new List<Step>()
                                });

                                if (innerStepElement.TryGetProperty("transit_details",
                                    out var innerTransitDetailsProperty))
                                {

                                }

                            }
                        }

                        step.TransitDetails = new TransitDetails();

                        //TODO: Look at transit_details
                        if (stepElement.TryGetProperty("transit_details", out var transitDetailsProperty))
                        {
                            if (transitDetailsProperty.TryGetProperty("line", out var line))
                            {
                                //var lineName = line.TryGetProperty("line", out var lineName)
                            }

                            //var line =

                        //This is messy - need to break down the individual pieces
                        //TODO: Add a helper that gets string using Try then returns GetString or null
                        //      Same for double

                        //step.TransitDetails = new TransitDetails
                        //{
                            //ArrivalStopName
                            //ArrivalStopLatitude
                            //ArrivalStopLongitude
                            //DepartureStopName
                            //DepartureStopLatitude
                            //DepartureStopLongitude
                            //    LineName = transitDetailsProperty.GetProperty("line_name").GetProperty("name")
                            //LineShortName
                            //LineVehicleName
                            //LineVehicleType
                            //NumStops =
                        //};
                        }
                    }
                }
            }

            return journey;
        }
    }
}
