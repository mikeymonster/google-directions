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
            if (string.IsNullOrWhiteSpace(_settings.GoogleDirectionsApiKey)) return null;

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
            uriBuilder.Append("&alternatives=true");
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

            uriBuilder.Append($"&key={_settings.GoogleDirectionsApiKey}");

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

            var jsonDoc = await JsonDocument.ParseAsync(jsonStream);

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

            foreach (var routeElement in jsonDoc.RootElement.GetProperty("routes").EnumerateArray())
            {
                //Debug.WriteLine($"Route {routeElement.Name}");

                var route = new Route
                {
                    Summary = routeElement.SafeGetString("summary"),
                    Warnings =
                        routeElement.TryGetProperty("warnings", out var warnings)
                        ? new List<string>(warnings
                            .EnumerateArray()
                            .Select(w =>
                            {
                                //TODO: Sort out loading of incorrect characters here 
                                Debug.WriteLine(w.GetString());
                                Debug.WriteLine((int)w.GetString()[44]);
                                Debug.WriteLine($"{(int)w.GetString()[44]:X}");
                                return w.GetString();
                            })
                            .ToList())
                    : new List<string>(),
                    Legs = new List<Leg>()
                };
                journey.Routes.Add(route);

                foreach (var legElement in routeElement.GetProperty("legs").EnumerateArray())
                {
                    var hasLegDistance = legElement.TryGetProperty("distance", out var legDistanceElement);
                    var hasLegDuration = legElement.TryGetProperty("duration", out var legDurationElement);

                    var leg = new Leg
                    {
                        StartAddress = legElement.SafeGetString("start_address"),
                        EndAddress = legElement.SafeGetString("end_address"),
                        Distance = hasLegDistance ? legDistanceElement.SafeGetInt32("value") : default,
                        DistanceString = hasLegDistance ? legDistanceElement.SafeGetString("text") : default,
                        Duration = hasLegDuration ? legDurationElement.SafeGetInt32("value") : default,
                        DurationString = hasLegDuration ? legDurationElement.SafeGetString("text") : default,
                        Steps = new List<Step>()
                    };
                    route.Legs.Add(leg);

                    foreach (var stepElement in legElement.GetProperty("steps").EnumerateArray())
                    {
                        var hasStepDistanceElement = stepElement.TryGetProperty("distance", out var stepDistanceElement);
                        var hasStepDurationElement = stepElement.TryGetProperty("duration", out var stepDurationElement);

                        var hasStepStartLocation = stepElement.TryGetProperty("start_location", out var stepStartLocationElement);
                        var hasStepEndLocation = stepElement.TryGetProperty("end_location", out var stepEndLocationElement);

                        var step = new Step
                        {
                            Distance = hasStepDistanceElement ? stepDistanceElement.SafeGetInt32("value") : default,
                            DistanceString = hasStepDistanceElement ? stepDistanceElement.SafeGetString("text") : default,
                            Duration = hasStepDistanceElement ? stepDurationElement.SafeGetInt32("value") : default,
                            DurationString = hasStepDistanceElement ? stepDurationElement.SafeGetString("text") : default,

                            StartLatitude = hasStepStartLocation ? stepStartLocationElement.SafeGetDouble("lat") : default,
                            StartLongitude = hasStepStartLocation ? stepStartLocationElement.SafeGetDouble("lng") : default,

                            EndLatitude = hasStepEndLocation ? stepEndLocationElement.SafeGetDouble("lat") : default,
                            EndLongitude = hasStepEndLocation ? stepEndLocationElement.SafeGetDouble("lng") : default,

                            Instructions = stepElement.SafeGetString("html_instructions"),
                            TravelMode = stepElement.SafeGetString("travel_mode"),

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
                                    DurationString = innerStepElement.GetProperty("duration").GetProperty("text").GetString(),

                                    StartLatitude = innerStepElement.GetProperty("start_location").GetProperty("lat").GetDouble(),
                                    StartLongitude = innerStepElement.GetProperty("start_location").GetProperty("lng").GetDouble(),

                                    EndLatitude = innerStepElement.GetProperty("end_location").GetProperty("lat").GetDouble(),
                                    EndLongitude = innerStepElement.GetProperty("end_location").GetProperty("lng").GetDouble(),

                                    Instructions = innerStepElement.SafeGetString("html_instructions"),

                                    Steps = new List<Step>()
                                });

                                if (innerStepElement.TryGetProperty("transit_details",
                                    out var innerTransitDetailsProperty))
                                {
                                }
                            }
                        }

                        var transitDetails = new TransitDetails();
                        step.TransitDetails = transitDetails;

                        //TODO: Look at transit_details
                        if (stepElement.TryGetProperty("transit_details", out var transitDetailsProperty))
                        {
                            transitDetails.NumStops = transitDetailsProperty.SafeGetInt32("num_stops");

                            if (transitDetailsProperty.TryGetProperty("line", out var lineProperty))
                            {
                                transitDetails.LineName = lineProperty.SafeGetString("name");
                                transitDetails.LineShortName = lineProperty.SafeGetString("short_name");

                                if (lineProperty.TryGetProperty("vehicle", out var vehicleProperty))
                                {
                                    transitDetails.LineVehicleName = vehicleProperty.SafeGetString("name");
                                    transitDetails.LineVehicleType = vehicleProperty.SafeGetString("type");
                                }
                            }

                            if (transitDetailsProperty.TryGetProperty("arrival_stop", out var arrivalStopProperty))
                            {
                                transitDetails.ArrivalStopName = arrivalStopProperty.SafeGetString("name");
                                transitDetails.ArrivalStopLatitude = arrivalStopProperty.GetProperty("location").SafeGetDouble("lat");
                                transitDetails.ArrivalStopLongitude = arrivalStopProperty.GetProperty("location").SafeGetDouble("long");
                            }

                            if (transitDetailsProperty.TryGetProperty("arrival_stop", out var departureStopProperty))
                            {
                                transitDetails.DepartureStopName = departureStopProperty.SafeGetString("name");
                                transitDetails.DepartureStopLatitude = departureStopProperty.GetProperty("location").SafeGetDouble("lat");
                                transitDetails.DepartureStopLongitude = departureStopProperty.GetProperty("location").SafeGetDouble("long");
                            }
                        }
                    }
                }
            }

            //Calculate distances
            CalculateJourneyDistances(journey);

            return journey;
        }

        private void CalculateJourneyDistances(Journey journey)
        {
            //TODO: Move to a TravelMode class
            const string TravelModeTransit = "TRANSIT";

            var distanceFromLastStop = 0;

            journey.Distance = journey.Routes.Sum(route => route.Legs.Sum(leg => leg.Distance));

            var distanceFound = false;
            foreach (var route in journey.Routes)
            {
                foreach (var leg in route.Legs.Reverse())
                {
                    foreach (var step in leg.Steps.Reverse())
                    {
                        //Loop backwards, accumulating the distance until we get to the last transit stop
                        if (step.TravelMode != TravelModeTransit)
                        {
                            distanceFromLastStop += step.Distance;
                        }
                        else
                        {
                            switch (step.TransitDetails.LineVehicleType)
                            {
                                /*
                                    //https://developers.google.com/maps/documentation/directions/overview#VehicleType
                                    RAIL	Rail.
                                    METRO_RAIL	Light rail transit.
                                    SUBWAY	Underground light rail.
                                    TRAM	Above ground light rail.
                                    MONORAIL	Monorail.
                                    HEAVY_RAIL	Heavy rail.
                                    COMMUTER_TRAIN	Commuter rail.
                                    HIGH_SPEED_TRAIN	High speed train.
                                    LONG_DISTANCE_TRAIN	Long distance train.
                                    BUS	Bus.
                                    INTERCITY_BUS	Intercity bus.
                                    TROLLEYBUS	Trolleybus.
                                    SHARE_TAXI	Share taxi is a kind of bus with the ability to drop off and pick up passengers anywhere on its route.
                                    FERRY	Ferry.
                                    CABLE_CAR	A vehicle that operates on a cable, usually on the ground. Aerial cable cars may be of the type GONDOLA_LIFT.
                                    GONDOLA_LIFT	An aerial cable car.
                                    FUNICULAR	A vehicle that is pulled up a steep incline by a cable. A Funicular typically consists of two cars, with each car acting as a counterweight for the other.
                                    OTHER	All other vehicles will return this type.
                                */
                                case "BUS":
                                case "INTERCITY_BUS":
                                    journey.DistanceFromNearestBusStop = distanceFromLastStop;
                                    return;
                                case "RAIL":
                                case "METRO_RAIL":
                                case "SUBWAY":
                                case "HIGH_SPEED_TRAIN":
                                case "LONG_DISTANCE_TRAIN":
                                case "HEAVY_RAIL":
                                    journey.DistanceFromNearestTrainStop = distanceFromLastStop;
                                    return;
                                default:
                                    Debug.WriteLine("Unexpected vehicle type ");
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
