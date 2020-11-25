using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using poc.Google.Directions.Interfaces;
using poc.Google.Directions.Models;

namespace poc.Google.Directions.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IDirectionsService _directionsService;
        private readonly IMagicLinkService _magicLinkService;
        private readonly IProviderDataService _providerDataService;

        private readonly ILogger<IndexModel> _logger;

        public Location HomeLocation { get; private set; }
        public IList<Provider> Providers { get; private set; }
        public IDictionary<string, Journey> Journeys { get; private set; }

        public IndexModel(
            IDirectionsService directionsService, 
            IMagicLinkService magicLinkService,
            IProviderDataService providerDataService,
            ILogger<IndexModel> logger)
        {
            _directionsService = directionsService ?? throw new ArgumentNullException(nameof(directionsService));
            _magicLinkService = magicLinkService ?? throw new ArgumentNullException(nameof(magicLinkService));
            _providerDataService = providerDataService ?? throw new ArgumentNullException(nameof(providerDataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task OnGet()
        {
            HomeLocation = await _providerDataService.GetHome();
            Providers = (await _providerDataService.GetProviders())
                .OrderBy(p => p.Name)
                .ToList();
            Journeys = new Dictionary<string, Journey>();
            
            foreach (var provider in Providers)
            {
                //Only call API for the first journey, until we know it works
                if (Journeys.Any())
                {
                    Journeys.Add(provider.Postcode, new Journey
                    {
                        Distance = 12.0,
                        DistanceFromNearestBusStop = 1.03,
                        DistanceFromNearestTrainStop = 0.4,
                        Steps = new List<string>
                        {
                            "Sample journey"
                        }
                    });
                    continue;
                }

                var journey = await _directionsService.GetDirections(
                    HomeLocation, 
                    new Location { Postcode = provider.Postcode, Latitude = provider.Latitude, Longitude = provider.Longitude});

                    Journeys.Add(provider.Postcode, journey);
            }
        }
    }

    /*
     *
        private readonly ICacheService _cacheService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public StudentController(
            ICacheService cacheService,
            IWebHostEnvironment hostEnvironment)
        {
            _cacheService = cacheService;
            _hostEnvironment = hostEnvironment;
        }

    private async Task<IList<SelectListItem>> GetQualificationsAsync()
        {
            const int cacheExpiryInSeconds = 1800;
            const string cacheKey = "Find_Qualifications";

            var qualifications = await _cacheService.GetOrCreateAsync(cacheKey,
                entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromSeconds(cacheExpiryInSeconds);

                    var providersFilePath = $"{_hostEnvironment.WebRootPath}\\js\\providers.json";
                    var jsonBytes = System.IO.File.ReadAllBytes(providersFilePath);

                    using var jsonDoc = JsonDocument.Parse(jsonBytes);
                    var root = jsonDoc.RootElement;

                    var qualificationsSection =  root.GetProperty("qualifications");

                    var list = qualificationsSection
                        .EnumerateObject()
                        //.Select(x => new {key = x.Name, val = x.Value.GetString()})
                        //.ToDictionary(item => item.key, item => item.val);
                        .Select(x => new SelectListItem(x.Value.GetString(), x.Name))
                        .ToList();
                    
                    //return Task.FromResult<IDictionary<string, string>>(dic);
                    return Task.FromResult(list);
                });

            return qualifications;
        }
     */
}
