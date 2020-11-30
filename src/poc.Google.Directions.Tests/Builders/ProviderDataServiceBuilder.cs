using poc.Google.Directions.Models;
using poc.Google.Directions.Services;

namespace poc.Google.Directions.Tests.Builders
{
    public class ProviderDataServiceBuilder
    {
        private readonly ProviderDataService _providerDataService;

        public ProviderDataServiceBuilder(ApiSettings settings = null)
        {
            _providerDataService = new ProviderDataService();
        }

        public ProviderDataService Build() => _providerDataService;
    }
}
