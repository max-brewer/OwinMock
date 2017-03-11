using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OwinMock
{
    public static class RequestBuilderHelpers
    {
        public static RequestBuilder Returns(this RequestBuilder requestBuilder, string responseBody) =>
            requestBuilder.Returns(new RequestWriter(responseBody).Write);

        public static RequestBuilder WithHeader(this RequestBuilder requestBuilder, string headerKey) =>
            requestBuilder.WithHeader(headerKey, YesPredicate);

        public static RequestBuilder ReturnsStatus(this RequestBuilder requestBuilder, HttpStatusCode statusCode) =>
            requestBuilder.ReturnsStatus((int) statusCode);

        private static bool YesPredicate(string arg) => true;

        private class RequestWriter
        {
            private readonly string _responseBody;

            public RequestWriter(string responseBody)
            {
                _responseBody = responseBody;
            }

            public Task Write(Stream stream)
            {
                return new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true }.WriteAsync(_responseBody);
            }
        }
    }
}