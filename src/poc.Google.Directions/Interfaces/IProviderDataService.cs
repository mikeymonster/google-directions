using System.Collections.Generic;
using System.Threading.Tasks;
using poc.Google.Directions.Models;

namespace poc.Google.Directions.Interfaces
{
    public interface IProviderDataService
    {
        public Task<Location> GetHome();
        public Task<IList<Provider>> GetProviders();
    }
}
