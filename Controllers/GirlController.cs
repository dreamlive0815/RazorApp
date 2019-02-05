
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http;

using Crawler.Girl;
using Crawler.Girl.Model;
using MyHttp;

namespace Controllers
{
    [Route("girl")]
    [ApiController]
    public class GirlController : Controller
    {
        IHttpClient _client;
        Girl girl;

        public GirlController(IHttpClient client)
        {
            _client = client;
            girl = new Girl(_client);
        }

        [HttpGet("album/{albumId:int}")]
        public JsonResult AlbumInfo(int albumId)
        {
            var album = girl.GetAlbumInfo(albumId.ToString());
            var urls = girl.GetAlbumImageUrls(album);
            urls.ForEach(u => {
                u = GetFinalImageUrl(u);
            });
            return Json(new {
                Album = album,
                Urls = urls,
            });
        }

        [HttpGet("gallery/{index:int=1}")]
        public JsonResult GalleryAlbums(int index)
        {
            if (index < 1) index = 1;
            var albums = girl.GetGalleryAlbums(index);
            albums.ForEach(a => {
                a.Cover = GetFinalImageUrl(a.Cover);
            });

            return Json(new {
                Index = index,
                Albums = albums,
            });
        }

        [HttpGet("girl/{girlId:int}")]
        public JsonResult GirlProfile(int girlId)
        {
            var tasks = new Task[2];
            var factory = Task.Factory;
            GirlProfile profile = null;
            List<Album> albums = null;

            tasks[0] = factory.StartNew(() => {
                profile = girl.GetGirlProfile(girlId.ToString());
                profile.Avatar = GetFinalImageUrl(profile.Avatar);
            });
            tasks[1] = factory.StartNew(() => {
                albums = girl.GetAlbums(girlId.ToString());
                albums.ForEach(a => {
                    a.Cover = GetFinalImageUrl(a.Cover);
                });
            });

            Task.WaitAll(tasks);
            return Json(new {
                Profile = profile,
                Albums = albums,
            });
        }

        [HttpGet("image/{girlId:int}/{albumId:int}/{index:int=0}")]
        public FileResult Image(int girlId, int albumId, int index)
        {
            var imageUrl = Girl.GetImageUrl(girlId.ToString(), albumId.ToString(), index);
            var stream = _client.GetStream(imageUrl);
            return File(stream, "image/jpeg", $"{girlId}-{albumId}-{index}.jpg");
        }

        [HttpGet("imagewithurl/{**url}")]
        public IActionResult Image(string url)
        {
            if(!Regex.IsMatch(url, "https://.+?/")) {
                return NotFound();
            }
            var u = Regex.Replace(url, "https://.+?/", "");
            var matches = Regex.Matches(u, "\\d+");
            if(matches.Count == 0) {
                return NotFound();
            }
            var fileName = string.Join("-", matches.Select(m => m.Value).Distinct());
            fileName = $"{fileName}.jpg";
            var stream = _client.GetStream(url);
            return File(stream, "image/jpeg", fileName);
        }

        /// <summary>
        /// 如果有防盗链 那么代理图片获取
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetFinalImageUrl(string url)
        {
            return url;
        }
    }
}