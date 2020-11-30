using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using poc.Google.Directions.Interfaces;
using poc.Google.Directions.Models;
// ReSharper disable StringLiteralTypo

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
                },
                new Provider
                {
                    Name = "Bicester School of Shopping",
                    Postcode = "OX26 6WD",
                    Town = "Bicester",
                    Longitude = -1.1566167358815447,
                    Latitude = 51.89217275852689,
                }
            }
                    .OrderBy(p => p.Name)
                    .ThenBy(p => p.Postcode)
                    .ToList()
                as IList<Provider>
            );
        }
    }
}