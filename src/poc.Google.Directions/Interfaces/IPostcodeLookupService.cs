using System.Threading.Tasks;
using poc.Google.Directions.Messages;

namespace poc.Google.Directions.Interfaces
{
    public interface IPostcodeLookupService
    {
        Task<PostcodeLookupResult> GetPostcodeInfo(string postcode);
    }
}
