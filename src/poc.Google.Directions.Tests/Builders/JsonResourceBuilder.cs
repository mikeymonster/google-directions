using System.Reflection;
using Wild.TestHelpers.Extensions;

namespace poc.Google.Directions.Tests.Builders
{
    // ReSharper disable StringLiteralTypo
    public abstract class JsonResourceBuilder
    {
        protected static string GetAsset(string assetName)
        {
            return $"{Assembly.GetExecutingAssembly().GetName().Name}.Assets.{assetName}"
                .ReadManifestResourceStreamAsString();
        }
    }
}
