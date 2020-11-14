using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using poc.Google.Directions.Interfaces;

namespace poc.Google.Directions.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IDirectionsService _directionsService;
        private readonly IMagicLinkService _magicLinkService;
        private readonly IProviderDataService _providerDataService;

        private readonly ILogger<IndexModel> _logger;

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

        public void OnGet()
        {

        }
    }
}
