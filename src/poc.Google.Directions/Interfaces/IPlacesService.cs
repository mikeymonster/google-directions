using System.IO;
using System.Threading.Tasks;
using poc.Google.Directions.Models;

namespace poc.Google.Directions.Interfaces
{
    public interface IPlacesService
    {
        public Task<Places> GetPlaces(Location nearTo, bool useTrainTransitMode = true, bool useBusTransitMode = true, int searchRadiusInMeters = 5000, bool rankByDistance = true);

        public Task<Places> BuildPlacesFromJson(Stream jsonStream);
    }
}
