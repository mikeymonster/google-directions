using System.Reflection;
using Wild.TestHelpers.Extensions;

namespace poc.Google.Directions.Tests.Builders
{
    // ReSharper disable StringLiteralTypo
    public class PostcodeLookupJsonBuilder
    {
        public string Build()
        {
            return GetAsset("testpostcoderesponse.json");
        }

        public string BuildNotFoundResponse()
        {
            return GetAsset("invalidpostcoderesponse.json");
        }

        public string BuildForTerminatedPostcode()
        {
            return GetAsset("testterminatedpostcoderesponse.json");
        }

        private static string GetAsset(string assetName)
        {
            return $"{Assembly.GetExecutingAssembly().GetName().Name}.Assets.{assetName}"
                .ReadManifestResourceStreamAsString();
        }
    }
}
