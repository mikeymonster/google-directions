using poc.Google.Directions.Models;

namespace poc.Google.Directions.Interfaces
{
    public interface IMagicLinkService
    {
        string CreateDirectionsLink(Location from, Location to, bool useTrainTransitMode = true, bool useBusTransitMode = true);
    }
}