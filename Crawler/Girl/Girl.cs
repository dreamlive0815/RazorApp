
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Crawler.Girl.Model;
using Extensions;
using HtmlAgilityPack;
using MyHttp;

namespace Crawler.Girl
{
    public class Girl
    {
        public static string BaseUri { get { return "https://www.nvshens.org"; } }

        public static string GetCoverUrl(string girlId, string albumId)
        {
            return $"https://t1.onvshen.com:85/gallery/{girlId}/{albumId}/cover/0.jpg";
        }

        public static string GetImageUrl(string girlId, string albumId, int index = 0, string extension = "jpg")
        {
            if (index < 0) index = 0;
            var i = index == 0 ? index.ToString() : string.Format("{0:D3}", index);
            return $"https://t1.onvshen.com:85/gallery/{girlId}/{albumId}/s/{i}.{extension}";
        }

        IHttpClient _client;

        public Girl(IHttpClient client)
        {
            _client = client;
            InitClient(_client);
        }

        private void InitClient(IHttpClient client)
        {
            HttpClientInitializer.Chrome(client);
            client.Headers.Referrer = BaseUri;
        }

        public string GetAlbumsUrl(string girlId)
        {
            return $"{BaseUri}/girl/{girlId}/album/";
        }

        public List<Album> GetAlbums(string girlId)
        {
            var url = GetAlbumsUrl(girlId);
            var s = _client.GetString(_client.BuildRequest(url));

            var list = new List<Album>();
            var nodes = s.XPath("//div[@class='igalleryli_div']//img");
            if (nodes == null)
                throw new Exception($"ID为{girlId}的模特作品不存在");
            var regex = new Regex("/(\\d+)/(\\d+)/cover/");
            foreach (var node in nodes) {
                var dataOri = node.GetAttributeValue("data-original", null);
                var src = node.GetAttributeValue("src", dataOri);
                var title = node.GetAttributeValue("title", null);
                var match = regex.Match(src);

                list.Add(new Album() {
                    Cover = src,
                    Id = match.Groups[2].Value,
                    GirlId = girlId,
                    Title = title,
                });
            }
            return list;
        }

        public string GetAlbumInfoUrl(string albumId)
        {
            return $"{BaseUri}/g/{albumId}/";
        }

        public Album GetAlbumInfo(string albumId)
        {
            var s = _client.GetString(_client.BuildRequest(GetAlbumInfoUrl(albumId)));
            var header = s.XPathSingle("//div[@class='albumTitle']");
            if (header == null)
                throw new Exception($"ID为{albumId}的相册不存在");

            var title = header.SelectSingleNodeHtml("./h1");
            var tags = header.SelectNodesHtml(".//ul[@id='utag']//a");
            var desc = header.SelectSingleNodeHtml("./div[@id='ddesc']");
            var infos = header.SelectSingleNodeHtml("./div[@id='dinfo']");
            var match = Regex.Match(infos, "(\\d+)张.+?(\\d+/\\d+/\\d+).+?(\\d+)");
            var src = s.XPathSingleAttr("//ul[@id='hgallery']/img", "src");
            var srcMatch = Regex.Match(src, "gallery/(\\d+)/(\\d+)/");

            return new Album() {
                Count = int.Parse(match.Groups[1].Value),
                Cover = GetCoverUrl(srcMatch.Groups[1].Value, albumId),
                CreateTime = match.Groups[2].Value,
                Description = desc,
                Hits = match.Groups[3].Value,
                Id = albumId,
                GirlId = srcMatch.Groups[1].Value,
                Tags = string.Join(",", tags),
                Title = title,
            };
        }

        public List<string> GetAlbumImageUrls(Album album)
        {
            var list = new List<string>();
            for (int i = 0; i < album.Count; i++) {
                list.Add(GetImageUrl(album.GirlId, album.Id, i));
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">页码</param>
        /// <returns></returns>
        public string GetGalleryAlbumsUrl(int index)
        {
            if (index < 1) index = 1;
            var i = index == 1 ? "" : $"{index}.html";
            return $"{BaseUri}/gallery/{i}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">页码 从1开始(1为空)</param>
        /// <returns></returns>
        public List<Album> GetGalleryAlbums(int index)
        {
            var s = _client.GetString(_client.BuildRequest(GetGalleryAlbumsUrl(index)));

            var list = new List<Album>();
            var nodes = s.XPath("//div[@class='galleryli_div']//img");
            if (nodes == null) return list;
            var regex = new Regex("/(\\d+)/(\\d+)/cover/");
            foreach (var node in nodes) {
                var src = node.GetAttributeValue("data-original", null);
                var title = node.GetAttributeValue("title", null);
                var match = regex.Match(src);

                list.Add(new Album() {
                    Cover = src,
                    Id = match.Groups[2].Value,
                    GirlId = match.Groups[1].Value,
                    Title = title,
                });
            }
            return list;
        }

        public string GetGirlProfileUrl(string girlId)
        {
            return $"{BaseUri}/girl/{girlId}";
        }

        public GirlProfile GetGirlProfile(string girlId)
        {
            var s = _client.GetString(_client.BuildRequest(GetGirlProfileUrl(girlId)));

            var infoBox = s.XPathSingle("//div[contains(@class, 'res_infobox')]");
            if (infoBox == null)
                throw new Exception($"ID为{girlId}的模特不存在");
            var name = infoBox.SelectSingleNodeHtml(".//div[contains(@class, 'div_h1')]/h1");
            var avatar = infoBox.SelectSingleNodeAttr(".//div[@class='infoleft_imgdiv']//img", "src");
            var infos = infoBox.SelectNodesHtml(".//div[@class='infodiv']//tr/td");
            var score = infoBox.SelectSingleNodeHtml(".//span[@id='span_score']");
            var desc = infoBox.SelectSingleNodeHtml("//div[@class='infocontent']/p");

            var dic = new Dictionary<string,string>();
            for(int i = 0; i < infos.Count; i += 2) {
                dic.Add(infos[i], infos[i + 1]);
            }
            var getInfo = new Func<string, string>(k => {
                var key = $"{k}：";
                if(!dic.ContainsKey(key)) return null;
                return dic[key];
            });
            
            return new GirlProfile() {
                Age = getInfo("年 龄"),
                Avatar = avatar,
                Birthday = getInfo("生 日"),
                Description = desc,
                Height = getInfo("身 高"),
                Hobby = getInfo("兴 趣"),
                Horoscope = getInfo("星 座"),
                Id = girlId,
                Job = getInfo("职 业"),
                Name = name,
                Origin = getInfo("出 生"),
                Size = getInfo("三 围"),
                Score = score,
                Weight = getInfo("体 重"),
            };
        }
    }
}