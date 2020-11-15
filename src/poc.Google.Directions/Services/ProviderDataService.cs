using System.Collections.Generic;
using System.Threading.Tasks;
using poc.Google.Directions.Interfaces;
using poc.Google.Directions.Models;

namespace poc.Google.Directions.Services
{
    public class ProviderDataService : IProviderDataService
    {
        public Task<Location> GetHome()
        {
            return Task.FromResult(new Location
            {
                Postcode = "CV1 2WT",
                Longitude = -1.508122,
                Latitude = 52.400997
            });
        }

        public Task<IList<Provider>> GetProviders()
        {
            return Task.FromResult(new List<Provider>
            {
                new Provider
                {
                    Name = "Solihull College & University Centre",
                    Postcode = "B91 1SB",
                    Town = "Solihull",
                    Longitude = -1.792148,
                    Latitude  = 52.409568
                },
                new Provider
                {
                    Name = "University College Birmingham",
                    Postcode = "B3 1JP",
                    Town = "Birmingham",
                    Longitude = -1.906807,
                    Latitude = 52.482263
                }
            } as IList<Provider>);
        }
    }
}