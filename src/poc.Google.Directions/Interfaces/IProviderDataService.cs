using System.Collections.Generic;
using poc.Google.Directions.Models;

namespace poc.Google.Directions.Interfaces
{
    public interface IProviderDataService
    {
        public Provider GetHome();
        public IList<Provider> GetProviders();
    }
}
