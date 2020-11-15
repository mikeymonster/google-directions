using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            Providers = await _providerDataService.GetProviders();
            HomeLocation = await _providerDataService.GetHome();
        }
    }
}
