using System.Threading.Tasks;
using poc.Google.Directions.Messages;
using poc.Google.Directions.Models;

namespace poc.Google.Directions.Interfaces
{
    public interface IPostcodeLookupService
    {
        Task<PostcodeLookupResult> GetPostcodeInfo(string postcode);
        Task<Location> GetPostcodeLocation(string postcode);
    }
}
