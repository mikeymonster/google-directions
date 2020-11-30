namespace poc.Google.Directions.Models
{
    public class Provider
    {
        public string Name { get; set; }
        public string Postcode { get; set; }
        public string Town { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string DirectionsLink { get; set; }

        public Location Location => new Location
        {
            Postcode = Postcode,
            Latitude = Latitude,
            Longitude = Longitude
        };
    }
}