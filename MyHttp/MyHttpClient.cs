using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MyHttp
{
    public class MyHttpClient : IHttpClient
    {
        HttpClient _client;
        HttpClientHandler _handler;
        CookieContainer _cookies;
        IRequestHeaderCollection _headers;

        public MyHttpClient()
        {
            _cookies = new CookieContainer()
            {
                PerDomainCapacity = 50,
            };

            _handler = new HttpClientHandler()
            {
                CookieContainer = _cookies,
                MaxAutomaticRedirections = 5,
            };

            _client = new HttpClient(_handler)
            {
            };

            _headers = new DefaultRequestHeaderCollection(_client, _client.DefaultRequestHeaders);
        }

        public override Uri BaseAddress
        {
            get { return _client.BaseAddress; }
            set { _client.BaseAddress = value; }
        }

        public override CookieContainer Cookies
        {
            get { return _cookies; }
        }

        public override DecompressionMethods Decompression
        {
            get { return _handler.AutomaticDecompression; }
            set { _handler.AutomaticDecompression = value; }
        }

        public override IRequestHeaderCollection Headers
        {
            get { return _headers; }
        }

        public override Stream GetStream(string requestUri)
        {
            var task = _client.GetStreamAsync(requestUri);
            task.Wait();
            return task.Result;
        }

        protected override HttpResponseMessage SendProto(HttpRequestMessage request)
        {
            var task = _client.SendAsync(request);
            task.Wait();
            return task.Result;
        }

        protected override Task<HttpResponseMessage> SendAsyncProto(HttpRequestMessage request)
        {
            return _client.SendAsync(request);
        }

        public override int Timeout 
        {
            get { return _client.Timeout.Milliseconds; }
            set { _client.Timeout =  TimeSpan.FromMilliseconds(value); }
        }

        public override IWebProxy Proxy
        {
            get { return _handler.Proxy; }
            set { _handler.Proxy = value; }
        }

        public override void Dispose()
        {
            _client.Dispose();
        }
    }
}