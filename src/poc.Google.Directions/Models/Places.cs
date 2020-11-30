using System.Collections.Generic;

namespace poc.Google.Directions.Models
{
    public class Places
    {
        public string RawJson { get; init; }
        public IList<Place> PlaceList { get; init; }
    }

    public class Place
    {
        public string Name { get; init; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public IList<string> Types { get; set; }
    }

}
