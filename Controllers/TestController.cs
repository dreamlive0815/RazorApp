using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http;

using MyHttp;

namespace Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {

        IHttpClientFactory _factory;

        public TestController(IHttpClientFactory clientFactory)
        {
            _factory = clientFactory;
            
        }

        [HttpGet]
        public string F()
        {
            
            
            var client = new MyHttpClient();
            HttpClientInitializer.Chrome(client);
            //client.Proxy = new FiddlerProxy();
            var s = client.GetString(client.BuildRequest("https://www.baidu.com"));

            return s;
        }
    }
}