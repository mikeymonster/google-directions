using System.Collections.Generic;

namespace poc.Google.Directions.Models
{
    public class Journey
    {
        public double Distance { get; init; }
        public double DistanceFromNearestBusStop { get; init; }
        public double DistanceFromNearestTrainStop { get; init; }
        public IList<string> Steps { get; init; }
        public string RawJson { get; init; }
    }
}
