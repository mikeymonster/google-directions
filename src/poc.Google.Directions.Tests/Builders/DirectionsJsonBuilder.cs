using System.IO;
using System.Threading.Tasks;
using poc.Google.Directions.Extensions;

namespace poc.Google.Directions.Tests.Builders
{
    // ReSharper disable StringLiteralTypo
    public class DirectionsJsonBuilder : JsonResourceBuilder
    {
        public string Build()
        {
            return GetAsset("testdirections.json");
        }

        public async Task<Stream> BuildStream()
        {
            return await GetAsset("testdirections.json")
                .BuildUtf8StreamStream();
        }
    }
}
