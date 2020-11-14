using poc.Google.Directions.Services;

namespace poc.Google.Directions.Tests.Builders
{
    public class MagicLinkServiceBuilder
    {
        private readonly MagicLinkService _magicLinkService;

        public MagicLinkServiceBuilder()
        {
            _magicLinkService = new MagicLinkService();
        }

        public MagicLinkService Build() => _magicLinkService;
    }
}
