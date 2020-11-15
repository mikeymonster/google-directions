using System.Threading.Tasks;
using poc.Google.Directions.Models;

namespace poc.Google.Directions.Interfaces
{
    public interface IDirectionsService
    {
        public Task<Journey> GetDirections(Location from, Location to);
    }
}