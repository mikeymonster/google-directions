using System.Text;
using poc.Google.Directions.Interfaces;

namespace poc.Google.Directions.Services
{
    public class MagicLinkService : IMagicLinkService
    {
        //See https://developers.google.com/maps/documentation/urls/get-started#forming-the-directions-url
        private const string BaseUrl = "https://www.google.com/maps/dir/?api=1&";

        public string CreateDirectionsLink()
        {
            var builder = new StringBuilder(BaseUrl);

            //https://stackoverflow.com/questions/45116011/generate-a-google-map-link-with-directions-using-latitude-and-longitude
            
            //var latDes = this.items[id].LongitudeWD;
            //var longDes = this.items[id].LatitudeWD;

            //var origin = "origin=" + tempLatitude + "," + tempLongitude;
            //var destination = "&destination=" + latDes + "," + longDes;
            //var newUrl = new URL(url + origin + destination);

            return builder.ToString();
        }
    }
}
