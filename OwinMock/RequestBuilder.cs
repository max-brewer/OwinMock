using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OwinMock
{
    public class RequestBuilder
    {
        public string Path { get; }
        public string Method { get; }
        public int StatusCode { get; private set; } = 200;
        public IDictionary<string, Func<string, bool>> RequestHeaders { get; } = new Dictionary<string, Func<string, bool>>();
        internal Func<Stream, Task> BodyWriter { get; private set; } = requestBody => Task.CompletedTask;
        public RequestBuilder(string method, string path)
        {
            Method = method;
            Path = path;
        }

        public RequestBuilder Returns(Func<Stream, Task> bodyWriter)
        {
            BodyWriter = bodyWriter;
            return this;
        }

        public RequestBuilder ReturnsStatus(int expectedStatusCode)
        {
            StatusCode = expectedStatusCode;
            return this;
        }

        public RequestBuilder WithHeader(string expectedHeader, Func<string, bool> headerPredicate)
        {
            RequestHeaders.Add(expectedHeader, headerPredicate);
            return this;
        }
    }
}