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
        User me;

        public PixivController(IHttpClient client)
        {
            this.client = client;
            pixiv = new Pixiv(client);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Redirect("/pixiv/index.html");
        }

        [HttpPost("bookmarknewillusts")]
        public IActionResult BookmarkNewIllustration([FromForm] string cookies, [FromForm] string pageId)
        {
            AssertLoggedIn(cookies);

            if (string.IsNullOrEmpty(pageId)) pageId = "1";
            var page = pixiv.GetBookmarkUsersNewIllustrations(pageId);
            page.Illustrations.ForEach(i => {
                i.Thumbnail = GetLocalProxyImageUrl(i.Thumbnail);
            });

            return Json(page);
        }

        [HttpPost("illust")]
        public IActionResult Illustration([FromForm] string cookies, [FromForm] string illustId)
        {
            AssertLoggedIn(cookies);

            var illust = pixiv.GetIllustrationInfo(illustId);
            illust.Thumbnail = GetLocalProxyImageUrl(illust.Thumbnail);
            illust.User.Avatar = GetLocalProxyImageUrl(illust.User.Avatar);
            var imageUrls = new List<string>();
            foreach (var url in illust.GetOriginalImageUrls()) {
                imageUrls.Add(GetLocalProxyImageUrl(url));
            }

            return Json(new {
                illust = illust,
                urls = imageUrls,
            });
        }

        [HttpPost("login")]
        public IActionResult Login([FromForm] string username, [FromForm] string password)
        {
            pixiv.Login(username, password);
            var cookies = pixiv.GetCookies();
            return Ok(cookies);
        }

        [HttpPost("ranklist")]
        public IActionResult RankList([FromForm] string cookies, [FromForm] string type, [FromForm] string pageId)
        {
            AssertLoggedIn(cookies);

            RankMode mode;
            switch (type)
            {
                case "daily": mode = RankMode.Daily; break;
                case "daily_r18": mode = RankMode.DailyR18; break;
                case "female": mode = RankMode.Female; break;
                case "female_r18": mode = RankMode.FemaleR18; break;
                case "male": mode = RankMode.Male; break;
                case "male_r18": mode = RankMode.MaleR18; break;
                case "monthly": mode = RankMode.Monthly; break;
                case "original": mode = RankMode.Original; break;
                case "rookie": mode = RankMode.Rookie; break;
                case "weekly": mode = RankMode.Weekly; break;
                case "weekly_r18": mode = RankMode.WeeklyR18; break;

                default: mode = RankMode.Daily; break;
            }
            if (string.IsNullOrEmpty(pageId)) pageId = "1";
            var page = pixiv.GetRankList(mode, pageId);
            page.Illustrations.ForEach(i => {
                i.Thumbnail = GetLocalProxyImageUrl(i.Thumbnail);
            });
            
            return Json(page);
        }

        [HttpGet("imagebyurl/{**url}")]
        public IActionResult Image(string url)
        {
            var match = Regex.Match(url, "^https://i.pximg.net.+/([^/]+\\.([^/]+))$");
            if(!match.Success) {
                return NotFound();
            }
            var stream = client.GetStream(url);
            var fileName = match.Groups[1].Value;
            var ext = match.Groups[2].Value;
            var type = "";
            switch (ext)
            {
                case "jpg": type = "image/jpeg"; break;
                case "png": type = "image/png"; break;

                default: type = "image/jpeg"; break;
            }
            
            return File(stream, type, fileName);
        }

        private void AssertLoggedIn(string cookies)
        {
            pixiv.InjectCookies(cookies);
            me = pixiv.GetMyStatus();
            if (me == null)
                throw new Exception("请先登录");
        }

        private string GetLocalProxyImageUrl(string url)
        {
            return $"/pixiv/imagebyurl/{url}";
        }
    }
}