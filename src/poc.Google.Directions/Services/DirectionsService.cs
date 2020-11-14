using poc.Google.Directions.Interfaces;
using poc.Google.Directions.Models;

namespace poc.Google.Directions.Services
{
    public class DirectionsService : IDirectionsService
    {
        private readonly ApiSettings _settings;

        private const string BaseUrl = "https://google";

        public DirectionsService(ApiSettings settings)
        {
            _settings = settings;
        }

        public Models.Directions GetDirections()
        {
            return new Models.Directions();
        }
    }
}
