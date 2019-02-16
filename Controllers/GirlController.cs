
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
using Newtonsoft.Json;

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

        [HttpGet]
        public RedirectResult Index()
        {
            return Redirect("/girl/index.html");
        }

        [HttpGet("album/{albumId:int}")]
        public JsonResult AlbumInfo(int albumId)
        {
            var album = girl.GetAlbumInfo(albumId.ToString());
            album.Cover = GetFinalImageUrl(album.Cover);
            var urls = girl.GetAlbumImageUrls(album);
            for(int i = 0; i < urls.Count; i++) {
                urls[i] = GetFinalImageUrl(urls[i]);
            }

            return Json(new {
                Album = album,
                Urls = urls,
            });
        }

        [HttpGet("bookmarkedgirls")]
        public JsonResult BookmarkedGirls()
        {
            var girls = new List<GirlProfile>() {
                new GirlProfile() {
                    Avatar = "https://img.onvshen.com:85/girl/22162/22162.jpg",
                    Id = "22162",
                    Name = "杨晨晨(sugar小甜心CC , sugar)",
                },
                new GirlProfile() {
                    Avatar = "https://img.onvshen.com:85/girl/19705/19705.jpg",
                    Id = "19705",
                    Name = "刘飞儿",
                },
                new GirlProfile() {
                    Avatar = "https://img.onvshen.com:85/girl/21501/21501.jpg",
                    Id = "21501",
                    Name = "夏美酱",
                },
            };

            return Json(girls);
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

        [HttpGet("profile/{girlId:int}")]
        public JsonResult GirlProfile(int girlId)
        {
            var profile = girl.GetGirlProfile(girlId.ToString());
            profile.Avatar = GetFinalImageUrl(profile.Avatar);

            return Json(profile);
        }

        [HttpGet("albumlist/{girlId:int}")]
        public JsonResult GirlAlbums(int girlId)
        {
            var albums = girl.GetAlbums(girlId.ToString());
            albums.ForEach(a => {
                a.Cover = GetFinalImageUrl(a.Cover);
            });

            return Json(albums);
        }

        [HttpGet("girl/{girlId:int}")]
        public JsonResult GirlProfileAndAlbums(int girlId)
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
            //return "/girl/imagewithurl/" + url;
        }
    }
}