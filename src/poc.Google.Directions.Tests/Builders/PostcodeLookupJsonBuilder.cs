
namespace poc.Google.Directions.Tests.Builders
{
    // ReSharper disable StringLiteralTypo
    public class PostcodeLookupJsonBuilder : JsonResourceBuilder
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
    }
}
