using poc.Google.Directions.Pages;
using Wild.TestHelpers.Extensions;
using Xunit;

namespace poc.Google.Directions.Tests
{
    public class IndexModelTests
    {
        [Fact]
        public void IndexMode_Constructor_Guards_Against_Null_Parameters()
        {
            typeof(IndexModel).ShouldNotAcceptNullOrBadConstructorArguments();
        }

        //[Fact]
        //public void IndexModel_Does_Something()
        //{
        //    var service = new IndexModel(
        //        Substitute.For<ICacheService>(),
        //        new DirectionsServiceBuilder().Build(),
        //        new MagicLinkServiceBuilder().Build(),
        //        new PostcodeLookupServiceBuilder().Build(),
        //        new ProviderDataServiceBuilder().Build(),
        //        Substitute.For<ILogger<IndexModel>>());

        //    //TODO: Test something
        //}
    }
}
