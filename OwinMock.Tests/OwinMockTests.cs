using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OwinMock.Tests
{
    public class OwinMockTests : IDisposable
    {
        const string Path = "/some/route/";
        private IDisposable _server;
        private readonly OwinMock _owinMock;
        private readonly HttpClient _httpClient;

        public OwinMockTests()
        {
            const int mockingPort = 4242;
            _owinMock = new OwinMock(mockingPort);
            _httpClient = new HttpClient {BaseAddress = new Uri($"http://localhost:{mockingPort}")};
        }

        [Fact]
        public async Task It_gives_a_404_with_no_setup()
        {
            _server = _owinMock.Serve();
            var httpClient = new HttpClient {BaseAddress = new Uri("http://localhost:4242")};

            var response = await httpClient.GetAsync("");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ItReturnsASimpleString()
        {
            const string expectedResponse = "Hello World!";

            _server = _owinMock.Serve(OwinMock.Get(Path).Returns(expectedResponse));

            var response = await _httpClient.GetAsync(Path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedResponse, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ItGivesMethodNotAllowedWhenYouAskForAnUnknownMethodOnAKnownRoute()
        {
            _server = _owinMock.Serve(OwinMock.Post(Path));

            var response = await _httpClient.GetAsync(Path);

            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task SupportsMultipleRoutes()
        {
            const string firstRoute = "firstRoute";
            const string secondRoute = "secondRoute/";

            _server = _owinMock.Serve(
                OwinMock.Get(firstRoute),
                OwinMock.Get(secondRoute));

            var firstResponse = _httpClient.GetAsync(firstRoute);
            var secondResponse = _httpClient.GetAsync(secondRoute);

            Assert.Equal(HttpStatusCode.OK, (await firstResponse).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await secondResponse).StatusCode);
        }

        [Fact]
        public async Task ItSupportsMultipleMethods()
        {
            _server = _owinMock.Serve(
                OwinMock.Get(Path),
                OwinMock.Post(Path));

            var firstResponse = _httpClient.GetAsync(Path);
            var secondResponse = _httpClient.PostAsync(Path, new StringContent(string.Empty));

            Assert.Equal(HttpStatusCode.OK, (await firstResponse).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await secondResponse).StatusCode);
        }

        [Fact]
        public async Task YouCanSpecifyAResponseCode()
        {
            const string route = "route/with/expected/status/code";
            const HttpStatusCode expectedStatusCode = HttpStatusCode.Forbidden;

            _server = _owinMock.Serve(OwinMock.Get(route).ReturnsStatus(expectedStatusCode));

            Assert.Equal(expectedStatusCode, (await _httpClient.GetAsync(route)).StatusCode);
        }

        [Fact]
        public async Task YouCanSpecifyRequiredHeadersAndGetABadRequestIfTheyAreAbsent()
        {
            const string expectedHeader = "MyHeader";

            _server = _owinMock.Serve(OwinMock.Get(Path).WithHeader(expectedHeader));

            var withoutHeader = await _httpClient.GetAsync(Path);
            _httpClient.DefaultRequestHeaders.Add(expectedHeader, "Some value");
            var withHeader = await _httpClient.GetAsync(Path);

            Assert.Equal(HttpStatusCode.OK, withHeader.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, withoutHeader.StatusCode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task YouCanSpecifyAValidationFunctionForAHeader(bool isValid)
        {
            var expectedStatusCode = isValid ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
            const string expectedHeader = "MyHeader";
            const string headerValue = "Some Value";
            string lastCall = null;
            var callCount = 0;

            Func<string, bool> headerPredicate = value =>
            {
                lastCall = value;
                callCount++;
                return isValid;
            };

            _server = _owinMock.Serve(OwinMock.Get(Path).WithHeader(expectedHeader, headerPredicate));
            
            _httpClient.DefaultRequestHeaders.Add(expectedHeader, headerValue);
            var response = await _httpClient.GetAsync(Path);

            Assert.Equal(expectedStatusCode, response.StatusCode);
            Assert.Equal(headerValue, lastCall);
            Assert.Equal(1, callCount);
        }

        /// Needs to work with all methods
        /// Work with multiple ports from one owin mock? Then it cleans them all up together.
        /// Which methods have request bodies? - Maybe have various request builders for different request types.
        /// Need to be able to interrogate the request for matching.
        /// * On query parameters
        /// * On headers
        /// * On request body
        /// Need to record the requests. - Then you can have a look later/do some assertions.
        /// Maybe get rid of dependencies on Microsoft.Owin and Owin.Hosting (Owin.Hosting can be a dependency, but not of the core module).
        /// Https?
        /// Various kinds of auth?
        /// Various helper methods for different request types
        /// Fluent API

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}

