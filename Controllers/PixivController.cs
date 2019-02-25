using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http;

using Crawler.Pixiv;
using Crawler.Pixiv.Model;
using MyHttp;
using Newtonsoft.Json;

namespace Controllers
{
    [Route("pixiv")]
    [ApiController]
    public class PixivController : Controller
    {
        IHttpClient client;
        Pixiv pixiv;

        public PixivController(IHttpClient client)
        {
            this.client = client;
            pixiv = new Pixiv(client);
        }

        [HttpPost]
        public IActionResult Index([FromForm] string cookies)
        {
            AssertLoggedIn(cookies);
            
            return Json(pixiv.GetRankList(RankMode.Daily, "1"));
        }

        [HttpPost("login")]
        public IActionResult Login([FromForm] string username, [FromForm] string password)
        {
            pixiv.Login(username, password);
            var cookies = pixiv.GetCookies();
            return Ok(cookies);
        }

        private void AssertLoggedIn(string cookies)
        {
            pixiv.InjectCookies(cookies);
            var me = pixiv.GetMyStatus();
            if (me == null)
                throw new Exception("请先登录");
        }
    }
}