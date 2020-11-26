using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using poc.Google.Directions.Interfaces;
using poc.Google.Directions.Models;
using Wild.TestHelpers.Extensions;

namespace poc.Google.Directions.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICacheService _cacheService;
        private readonly IDirectionsService _directionsService;
        private readonly IMagicLinkService _magicLinkService;
        private readonly IPostcodeLookupService _postcodeLookupService;
        private readonly IProviderDataService _providerDataService;

        public const int CacheExpiryInSeconds = 43200; //12 hours

        private readonly ILogger<IndexModel> _logger;

        public Location HomeLocation { get; private set; }

        [BindProperty]
        [Required]
        public string Postcode { get; private set; }

        public IList<Provider> Providers { get; private set; }

        public IDictionary<string, Journey> Journeys { get; private set; }

        private static bool _preloaded;

        public IndexModel(
            ICacheService cacheService,
            IDirectionsService directionsService,
            IMagicLinkService magicLinkService,
            IPostcodeLookupService postcodeLookupService,
            IProviderDataService providerDataService,
            ILogger<IndexModel> logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _directionsService = directionsService ?? throw new ArgumentNullException(nameof(directionsService));
            _magicLinkService = magicLinkService ?? throw new ArgumentNullException(nameof(magicLinkService));
            _postcodeLookupService = postcodeLookupService ?? throw new ArgumentNullException(nameof(postcodeLookupService));
            _providerDataService = providerDataService ?? throw new ArgumentNullException(nameof(providerDataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (!_preloaded)
            {
                Task.Run(PreloadAssets).Wait();
            }
        }

        public async Task OnGet()
        {
            HomeLocation ??= await _providerDataService.GetHome();

            //Default location 
            Postcode = HomeLocation.Postcode;

            await SearchForProviders();

            await SearchForJourneys(HomeLocation);

            //Journeys = new Dictionary<string, Journey>();

            //foreach (var provider in Providers)
            //{
            //    //Only call API for the first journey, until we know it works
            //    if (Journeys.Any())
            //    {
            //        Journeys.Add(provider.Postcode, new Journey
            //        {
            //            Distance = 12.0,
            //            DistanceFromNearestBusStop = 1.03,
            //            DistanceFromNearestTrainStop = 0.4,
            //            Steps = new List<string>
            //            {
            //                "Sample journey"
            //            }
            //        });
            //        continue;
            //    }

            //    var journey = await _directionsService.GetDirections(
            //        HomeLocation, 
            //        new Location { Postcode = provider.Postcode, Latitude = provider.Latitude, Longitude = provider.Longitude});

            //        Journeys.Add(provider.Postcode, journey);
            //}
        }

        public async Task<IActionResult> OnPost([FromForm] string postcode)
        {
            Postcode = postcode;
            Location location = null;

            if (ModelState.IsValid)
            {
                //Messy - validates postcode, if ok then gets location and validates that
                if (Postcode == null)
                {
                    ModelState.AddModelError(nameof(Postcode), "You must enter a postcode");
                }
                else
                {
                    location = await GetLocationForPostcode(postcode);
                    if (location == null)
                    {
                        ModelState.AddModelError(nameof(Postcode), "You must enter a valid postcode");
                    }
                }

                if (ModelState.IsValid)
                {
                    await SearchForProviders();
                    await SearchForJourneys(location);
                }
            }

            return Page();
        }

        private async Task PreloadAssets()
        {
            if (_preloaded) return;

            /* Preload postcode lookup results */
            HomeLocation = await _providerDataService.GetHome();
            var locationCacheKey = CreateLocationCacheKey(HomeLocation.Postcode);
            _cacheService.Set(locationCacheKey, HomeLocation, TimeSpan.FromSeconds(CacheExpiryInSeconds));

            /* Preload journey results */
            var basePath = Assembly.GetExecutingAssembly().GetName().Name;
            const string destinationPostcode = "B91 1SB";
            
            var json = $"{basePath}.Assets.saved_results_{MakePostcodeKey(HomeLocation.Postcode)}_to_{MakePostcodeKey(destinationPostcode)}.json"
                .ReadManifestResourceStreamAsString();

            var journey = await _directionsService.BuildJourneyFromJsonString(json);

            var journeyCacheKey = CreateJourneyCacheKey(HomeLocation.Postcode, destinationPostcode);
            _cacheService.Set(journeyCacheKey, journey, TimeSpan.FromSeconds(CacheExpiryInSeconds));

            //A bit hacky, and not thread safe - set a static flag so this will only run once
            _preloaded = true;
        }

        private static string MakePostcodeKey(string postcode) =>
            postcode.ToLower().Replace(' ', '_');

        private static string CreateJourneyCacheKey(string fromPostcode, string toPostcode) =>
            $"__Journey_{MakePostcodeKey(fromPostcode)}_{MakePostcodeKey(toPostcode)}__";

        private static string CreateLocationCacheKey(string postcode) =>
            $"__Location_{MakePostcodeKey(postcode)}__";

        private async Task<Location> GetLocationForPostcode(string postcode)
        {
            var cacheKey = CreateLocationCacheKey(postcode);
            var location = _cacheService.Get<Location>(cacheKey);

            if (location == null)
            {
                location = await _postcodeLookupService.GetPostcodeLocation(postcode);

                if (location != null)
                {
                    _cacheService.Set(cacheKey, location, TimeSpan.FromSeconds(CacheExpiryInSeconds));
                }
            }

            return location;
        }

        private async Task SearchForProviders()
        {
            Providers = (await _providerDataService.GetProviders())
                .OrderBy(p => p.Name)
                .ToList();
        }

        private async Task SearchForJourneys(Location location)
        {
            Providers = (await _providerDataService.GetProviders())
                .OrderBy(p => p.Name)
                .ToList();

            Journeys = new Dictionary<string, Journey>();

            foreach (var provider in Providers)
            {
                //Only call API for the first journey, until we know it works
                if (Journeys.Any())
                {
                    Journeys.Add(provider.Postcode, CreateFakeJourney());
                    continue;
                }

                var cacheKey = CreateJourneyCacheKey(location.Postcode, provider.Postcode);
                var journey = _cacheService.Get<Journey>(cacheKey);

                if (journey == null)
                {
                    journey = await _directionsService.GetDirections(
                        location,
                        new Location
                        {
                            Postcode = provider.Postcode,
                            Latitude = provider.Latitude,
                            Longitude = provider.Longitude
                        });
                    _cacheService.Set(cacheKey, journey, TimeSpan.FromSeconds(CacheExpiryInSeconds));
                }

                Journeys.Add(provider.Postcode, journey);
            }
        }

        private static Journey CreateFakeJourney() =>
            new Journey
            {
                Distance = 12.0,
                DistanceFromNearestBusStop = 1.03,
                DistanceFromNearestTrainStop = 0.4,
                Steps = new List<string>
                {
                    "Sample journey"
                }
            };
    }
}
