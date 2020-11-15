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
            _logger = logger;
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
}
