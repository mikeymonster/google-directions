using System;
using System.Collections.Generic;
using System.Net.Http;
using NSubstitute;
using poc.Google.Directions.Interfaces;
using poc.Google.Directions.Services;
using Wild.TestHelpers.HttpClient;

namespace poc.Google.Directions.Tests.Builders
{
    // ReSharper disable UnusedMember.Global
    public class ServiceBuilder
    {
        public IPostcodeLookupService BuildPostcodeLookupService(
            Uri baseUri,
            IHttpClientFactory httpClientFactory = null)
        {
            httpClientFactory ??= Substitute.For<IHttpClientFactory>();

            return new PostcodeLookupService(baseUri, httpClientFactory);
        }

        public IPostcodeLookupService BuildPostcodeLookupService(
            Uri baseUri,
            string targetUriFragment,
            PostcodeLookupJsonBuilder dataBuilder)
        {
            var targetUri = new Uri(baseUri, targetUriFragment);

            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory
                .CreateClient()
                .Returns(new TestHttpClientFactory()
                    .CreateHttpClient(targetUri,
                        dataBuilder.Build()));

            return new PostcodeLookupService(baseUri, httpClientFactory);
        }

        public IPostcodeLookupService BuildPostcodeLookupService(
            Uri baseUri,
            IDictionary<Uri, HttpResponseMessage> responseMessages)
        {
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory
                .CreateClient()
                .Returns(new TestHttpClientFactory()
                    .CreateHttpClient(responseMessages));

            return new PostcodeLookupService(baseUri, httpClientFactory);
        }
    }
}
