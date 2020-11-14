using poc.Google.Directions.Models;
using poc.Google.Directions.Services;

namespace poc.Google.Directions.Tests.Builders
{
    public class DirectionsServiceBuilder
    {
        private readonly DirectionsService _directionsService;

        public DirectionsServiceBuilder(ApiSettings settings = null)
        {
            settings ??= new ApiSettings
            {
                GoogleApiKey = "test_key"
            };

            _directionsService = new DirectionsService(settings);
        }

        public DirectionsService Build() => _directionsService;
    }
}
