using System.Collections.Generic;
using poc.Google.Directions.Interfaces;
using poc.Google.Directions.Models;

namespace poc.Google.Directions.Services
{
    public class ProviderDataService : IProviderDataService
    {
        public Provider GetHome()
        {
            return new Provider
            {
                Name = "ESFA Coventry",
                Postcode = "CV1 2WT",
                Town = "Coventry",
                Longitude = -1.508122,
                Latitude = 52.400997
            };
        }

        public IList<Provider> GetProviders()
        {
            return new List<Provider>
            {
                new Provider
                {
                    Name = "SOLIHULL COLLEGE & UNIVERSITY CENTRE",
                    Postcode = "B91 1SB",
                    Town = "Solihull",
                    Longitude = -1.792148,
                    Latitude  = 52.409568
                },
                new Provider
                {
                    Name = "UNIVERSITY COLLEGE BIRMINGHAM",
                    Postcode = "B3 1JP",
                    Town = "Birmingham",
                    Longitude = -1.906807,
                    Latitude = 52.482263
                }
            };
        }
    }
}