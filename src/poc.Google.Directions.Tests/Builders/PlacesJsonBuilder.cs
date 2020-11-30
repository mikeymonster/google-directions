using System.IO;
using System.Threading.Tasks;
using poc.Google.Directions.Extensions;

namespace poc.Google.Directions.Tests.Builders
{
    // ReSharper disable StringLiteralTypo
    public class PlacesJsonBuilder : JsonResourceBuilder
    {
        public string Build()
        {
            return GetAsset("testplaces.json");
        }

        public async Task<Stream> BuildStream()
        {
            return await GetAsset("testplaces.json")
                .BuildUtf8StreamStream();
        }
    }
}
