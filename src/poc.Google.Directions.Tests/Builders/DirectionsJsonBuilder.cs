
namespace poc.Google.Directions.Tests.Builders
{
    // ReSharper disable StringLiteralTypo
    public class DirectionsJsonBuilder : JsonResourceBuilder
    {
        public string Build()
        {
            return GetAsset("testdirections.json");
        }
    }
}
