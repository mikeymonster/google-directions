using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using poc.Google.Directions.Interfaces;
using poc.Google.Directions.Messages;

namespace poc.Google.Directions.Services
{
    public class PostcodeLookupService : IPostcodeLookupService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Uri _postcodeRetrieverBaseUri;

        public PostcodeLookupService(Uri baseUri, IHttpClientFactory httpClientFactory)
        {
            _postcodeRetrieverBaseUri = baseUri != null && !string.IsNullOrWhiteSpace(baseUri.AbsolutePath)
                ? baseUri
                : throw new ArgumentException("Value of must not be null or whitespace.", nameof(baseUri));

            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<PostcodeLookupResult> GetPostcodeInfo(string postcode)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var lookupUri = new Uri(_postcodeRetrieverBaseUri, $"postcodes/{FormatPostcode(postcode)}");
            var isTerminated = false;

            var responseMessage = await httpClient.GetAsync(lookupUri);

            if (responseMessage.StatusCode != HttpStatusCode.OK)
            {
                //Fallback to terminated postcode search
                var terminatedPostcodeLookupUri = new Uri(_postcodeRetrieverBaseUri, $"terminated_postcodes/{FormatPostcode(postcode)}");
                responseMessage = await httpClient.GetAsync(terminatedPostcodeLookupUri);

                if (responseMessage.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                isTerminated = true;
            }

            var content = await responseMessage.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PostcodeLookupResponse>(content);
            result.Result.IsTerminatedPostcode = isTerminated;

            return result.Result;
        }

        private static string FormatPostcode(string postcode)
        {
            return Uri.EscapeUriString(postcode.Trim().ToUpper());
        }
    }
}
