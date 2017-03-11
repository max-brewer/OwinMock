using System;
using Microsoft.Owin.Hosting;
using Owin;

namespace OwinMock
{
    public class OwinMock
    {
        private readonly int _portNumber;

        public OwinMock(int portNumber)
        {
            _portNumber = portNumber;
        }

        private void Startup(IAppBuilder appBuilder, object configuredRequests)
        {
            appBuilder.Use(typeof(ServerMockleware), configuredRequests);
        }

        public IDisposable Serve(params RequestBuilder[] configuredRequests)
        {
            return WebApp.Start($"http://localhost:{_portNumber}", 
                appBuilder => Startup(appBuilder, configuredRequests));
        }

        public static RequestBuilder Get(string path)
        {
            return new RequestBuilder("GET", path);
        }

        public static RequestBuilder Post(string path)
        {
            return new RequestBuilder("POST", path);
        }
    }
}