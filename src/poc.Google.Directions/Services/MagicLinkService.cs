using System.Text;
using poc.Google.Directions.Interfaces;
using poc.Google.Directions.Models;

namespace poc.Google.Directions.Services
{
    public class MagicLinkService : IMagicLinkService
    {
        //See https://developers.google.com/maps/documentation/urls/get-started#forming-the-directions-url
        private const string BaseUrl = "https://www.google.com/maps/dir/?api=1&";

        public string CreateDirectionsLink(Location from, Location to, bool useTrainTransitMode = true, bool useBusTransitMode = true)
        {
            var uriBuilder = new StringBuilder(BaseUrl);

            //https://stackoverflow.com/questions/45116011/generate-a-google-map-link-with-directions-using-latitude-and-longitude

            //https://www.google.com/maps/dir/B91+1NG,+Solihull/Solihull+College+%26+University+Centre+Blossomfield+Campus,+Solihull/@52.4113588,-1.795049,17z/data=!3m1!4b1!4m14!4m13!1m5!1m1!1s0x4870b9e9b63f91ef:0x2a0e6b03104f3776!2m2!1d-1.7921997!2d52.4131839!1m5!1m1!1s0x4870b9c249edef4d:0xa9adeba9e68f74c!2m2!1d-1.7924987!2d52.4092323!3e3

            //var latDes = this.items[id].LongitudeWD;
            //var longDes = this.items[id].LatitudeWD;

            //uriBuilder.Append("&map_action=map");
            //TODO: Use postcodes/address?
            uriBuilder.Append($"origin={from.Latitude},{from.Longitude}");
            uriBuilder.Append($"&destination={to.Latitude},{to.Longitude}");
            //uriBuilder.Append("&region=uk");
            //TODO: try without this
            uriBuilder.Append("&travelmode=transit");
            //uriBuilder.Append("&layer=transit");


            return uriBuilder.ToString();
        }
    }
}
