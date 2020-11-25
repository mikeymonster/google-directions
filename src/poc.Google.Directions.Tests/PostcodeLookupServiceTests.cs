using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using poc.Google.Directions.Messages;
using poc.Google.Directions.Services;
using poc.Google.Directions.Tests.Builders;
using Wild.TestHelpers.Extensions;
using Wild.TestHelpers.HttpClient;
using Xunit;

namespace poc.Google.Directions.Tests
{
    public class PostcodeLookupServiceTests
    {
        [Fact]
        public void PostcodeLookupService_Constructor_Guards_Against_NullParameters()
        {
            typeof(PostcodeLookupService).ShouldNotAcceptNullOrBadConstructorArguments();
        }

        [Fact]
        public async Task PostcodeLookupService_Gets_Postcode_Successfully()
        {
            const string postcode = "CV1 2WT";
            const string postcodeUriFragment = "postcodes/CV1%202WT";

            var service = new PostcodeLookupServiceBuilder()
                .Build(
                    postcodeUriFragment,
                    new PostcodeLookupJsonBuilder());

            var result = await service.GetPostcodeInfo(postcode);

            VerifyPostcodeLookupResult(result, "CV1 2WT", 52.400997, -1.508122);
        }

        [Fact]
        public async Task PostcodeLookupService_Gets_Terminated_Postcode_Successfully()
        {
            const string postcode = "S70 2YW";
            const string postcodePart = "S70%202YW";
            var postcodeUri = new Uri(PostcodeLookupService.BaseUri, $"postcodes/{postcodePart}");
            var terminatedPostcodeUri = new Uri(PostcodeLookupService.BaseUri, $"terminated_postcodes/{postcodePart}");

            var builder = new PostcodeLookupJsonBuilder();
            var testHttpClientFactory = new TestHttpClientFactory();

            var responses = new Dictionary<Uri, HttpResponseMessage>
            {
                {
                    postcodeUri, testHttpClientFactory.CreateFakeResponse(builder.BuildNotFoundResponse(),
                                                                          responseCode: HttpStatusCode.NotFound)
                },
                {
                    terminatedPostcodeUri, testHttpClientFactory.CreateFakeResponse(builder.BuildForTerminatedPostcode())
                }
            };

            var service = new PostcodeLookupServiceBuilder()
                .Build(responses);

            var result = await service.GetPostcodeInfo(postcode);

            VerifyPostcodeLookupResult(result, "S70 2YW", 53.551618, -1.482797, true);
        }

        [Fact]
        public async Task PostcodeLookupService_Returns_Null_For_Nonexistent_Postcode()
        {
            const string postcode = "NON CDE";
            const string postcodePart = "NON%20CDE";
            var postcodeUri = new Uri(PostcodeLookupService.BaseUri, $"postcodes/{postcodePart}");
            var terminatedPostcodeUri = new Uri(PostcodeLookupService.BaseUri, $"terminated_postcodes/{postcodePart}");

            var builder = new PostcodeLookupJsonBuilder();
            var testHttpClientFactory = new TestHttpClientFactory();

            var responses = new Dictionary<Uri, HttpResponseMessage>
            {
                {
                    postcodeUri, testHttpClientFactory.CreateFakeResponse(builder.BuildNotFoundResponse(),
                                                                          responseCode: HttpStatusCode.NotFound)
                },
                {
                    terminatedPostcodeUri, testHttpClientFactory.CreateFakeResponse(builder.BuildNotFoundResponse(),
                                                                                    responseCode: HttpStatusCode.NotFound)
                }
            };

            var service = new PostcodeLookupServiceBuilder()
                .Build(responses);

            var result = await service.GetPostcodeInfo(postcode);

            //result.Verify("S70 2YW", "53.551618", "-1.482797", true);
            result.Should().BeNull();
        }
        
        private static void VerifyPostcodeLookupResult(PostcodeLookupResult postcodeLookupResult, string postcode, double latitude, double longitude, bool isTerminated = false)
        {
            postcodeLookupResult.Should().NotBeNull();
            postcodeLookupResult.Postcode.Should().Be(postcode);
            postcodeLookupResult.Latitude.Should().Be(latitude);
            postcodeLookupResult.Longitude.Should().Be(longitude);
            postcodeLookupResult.IsTerminatedPostcode.Should().Be(isTerminated);
        }
    }
}
