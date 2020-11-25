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
    public class PostcodeLookupServiceBuilder
    {
        public IPostcodeLookupService Build(
            IHttpClientFactory httpClientFactory = null)
        {
            httpClientFactory ??= Substitute.For<IHttpClientFactory>();

            return new PostcodeLookupService(httpClientFactory);
        }

        public IPostcodeLookupService Build(
            string targetUriFragment,
            PostcodeLookupJsonBuilder dataBuilder)
        {
            var targetUri = new Uri(PostcodeLookupService.BaseUri, targetUriFragment);

            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory
                .CreateClient()
                .Returns(new TestHttpClientFactory()
                    .CreateHttpClient(targetUri,
                        dataBuilder.Build()));

            return new PostcodeLookupService(httpClientFactory);
        }

        public IPostcodeLookupService Build(
            IDictionary<Uri, HttpResponseMessage> responseMessages)
        {
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory
                .CreateClient()
                .Returns(new TestHttpClientFactory()
                    .CreateHttpClient(responseMessages));

            return new PostcodeLookupService(httpClientFactory);
        }
    }
}
