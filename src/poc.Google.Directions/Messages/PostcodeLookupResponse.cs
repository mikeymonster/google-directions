using System.Text.Json.Serialization;

namespace poc.Google.Directions.Messages
{
    public class PostcodeLookupResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }
        [JsonPropertyName("result")]
        public PostcodeLookupResult Result { get; set; }
    }
}