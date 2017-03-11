using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;

//using AppFunc = Func<IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace OwinMock
{
    public class ServerMockleware : OwinMiddleware
    {
        private readonly IEnumerable<RequestBuilder> _requestBuilders;

        public ServerMockleware(OwinMiddleware next, IEnumerable<RequestBuilder> requestBuilders) : base(next)
        {
            _requestBuilders = requestBuilders;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var path = (string)context.Environment["owin.RequestPath"];

            if (path.EndsWith("/"))
            {
                path = path.TrimEnd('/');
            }

            var responses = _requestBuilders
                .Where(r => r.Path.Trim('/').Equals(path.Trim('/'), StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!responses.Any())
            {
                await Next.Invoke(context);
                return;
            }

            var response = responses.SingleOrDefault(r => r.Method == context.Request.Method);

            if (response == null)
            {
                context.Response.StatusCode = (int) HttpStatusCode.MethodNotAllowed;
                return;
            }

            var containsRequiredHeaders = response.RequestHeaders.All(header =>
            {
                string[] actualValues;
                return context.Request.Headers.TryGetValue(header.Key, out actualValues)
                       && actualValues.Any(header.Value);
            });

            context.Response.StatusCode = containsRequiredHeaders
                ? response.StatusCode
                : (int) HttpStatusCode.BadRequest;

            response.BodyWriter(context.Response.Body);
        }
    }
}
