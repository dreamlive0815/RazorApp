using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http;

using MyHttp;

namespace Controllers
{
    [Route("/")]
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
            return "HHH";
        }
    }
}