using poc.Google.Directions.Models;

namespace poc.Google.Directions.Tests.Builders
{
    public class LocationBuilder
    {
        public static Location FromLocation => new Location
        {
            Postcode = "CV1 2WT",
            Longitude = -1.508122,
            Latitude = 52.400997
        };

        public static Location ToLocation => new Location
        {
            Postcode = "B91 1SB",
            Longitude = -1.792148,
            Latitude = 52.409568
        };
    }
}
