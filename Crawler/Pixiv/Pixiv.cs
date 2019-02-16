
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

using Crawler.Pixiv.Model;
using Extensions;
using HtmlAgilityPack;
using MyHttp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Crawler.Pixiv
{
    public class Pixiv
    {
        public static string BaseUri { get { return "https://www.pixiv.net"; } }

        public static string AccountBaseUri { get { return "https://accounts.pixiv.net"; } }

        IHttpClient _client;
        bool isLoggedIn;
        string token;
        User me;

        public bool IsLoggedIn { get { return isLoggedIn; } }

        public Pixiv(IHttpClient client)
        {
            _client = client;
            InitClient(_client);
        }

        private void InitClient(IHttpClient client)
        {
            HttpClientInitializer.Chrome(client);
            client.Headers.Referrer = BaseUri;
        }

        public void Login(string username, string password)
        {
            if (isLoggedIn) return;
            var loginUrl = $"{AccountBaseUri}/login";
            var s = _client.GetString(_client.BuildRequest(loginUrl));
            var match = Regex.Match(s, "name=\"post_key\".+?value=\"(.+?)\"");
            if (!match.Success)
                throw new Exception("无法获取post_key");
            s = _client.GetString(_client.BuildRequest(loginUrl, new Dictionary<string, object>(){
                { "pixiv_id", username },
                { "password", password },
                { "post_key", match.Groups[1].Value },
                { "lang", "zh" },
                { "source", "pc" },
                { "return_to", "https://www.pixiv.net/" },
            }));
            if (GetMyStatus(s) == null)
                throw new Exception("账号或密码有误,登录失败");
        }

        public string GetBookmarkNewIllustrationsUrl(string pageId)
        {
            return $"{BaseUri}/bookmark_new_illust.php?p={pageId}";
        }

        public IllustrationsPage GetBookmarkNewIllustrations(string pageId)
        {
            AssertLoggedIn();

            var url = GetBookmarkNewIllustrationsUrl(pageId);
            var s = _client.GetString(_client.BuildRequest(url));

            var pageIdContainer = s.XPathSingle("//div[@class='pager-container']");
            var prevUri = pageIdContainer.SelectSingleNodeAttr("./span[@class='prev']/a", "href");
            var currentPageId = pageIdContainer.SelectSingleNodeHtml(".//li[@class='current']");
            var nextUri = pageIdContainer.SelectSingleNodeAttr("./span[@class='next']/a", "href");
            var regex = new Regex("p=(\\d+)");
            var getPageId = new Func<string, string>(uri => {
                if (uri == null) return null;
                if (uri == "?") return "1";
                var match = regex.Match(uri);
                return match.Success ? match.Groups[1].Value : null;
            });
            var prevPageId = getPageId(prevUri);
            var nextPageId = getPageId(nextUri);
            
            var items = s.XPathSingleAttr("//div[@id='js-mount-point-latest-following']", "data-items");
            if (items == null)
                throw new Exception("无法获取最新插画信息");
            items = HttpUtility.HtmlDecode(items);
            var arr = JArray.Parse(items);

            var page = new IllustrationsPage() {
                NextPageId = nextPageId,
                PageId = currentPageId,
                PrevPageId = prevPageId,
            };
            foreach (var token in arr) {
                var illust = new Illustration() {
                    Bookmarked = token.Val<bool>("isBookmarked"),
                    Count = token.Val<int>("pageCount"),
                    Height = token.Val<string>("height"),
                    Id = token.Val<string>("illustId"),
                    Tags = token["tags"].AsList<string>(),
                    Thumbnail = token.Val<string>("url"),
                    Title = token.Val<string>("illustTitle"),
                    User = new User() {
                        Id = token.Val<string>("userId"),
                        Name = token.Val<string>("userName"),
                    },
                    Width = token.Val<string>("width"),
                };
                page.Illustrations.Add(illust);
            }
        
            return page;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="showPublic">true显示公开关注,false显示私人关注</param>
        /// <returns></returns>
        public string GetBookmarkUsersUrl(string pageId, bool showPublic = true)
        {
            var rest = showPublic ? "show" : "hide";
            return $"{BaseUri}/bookmark.php?type=user&rest={rest}&p={pageId}";
        }

        public BookmarkUsersPage GetBookmarkUsers(string pageId, bool showPublic = true)
        {
            AssertLoggedIn();

            var url = GetBookmarkUsersUrl(pageId, showPublic);
            var s = _client.GetString(_client.BuildRequest(url));

            var doc = s.XPathSingle(".");
            var nodes = doc.SelectNodes(".//div[@class='members']//div[@class='userdata']");
            var pageNav = doc.SelectSingleNode(".//div[@class='_pager-complex']");
            var prevUri = pageNav.SelectSingleNodeAttr(".//a[@rel='prev']", "href");
            var currentPageId = pageNav.SelectSingleNodeHtml(".//li[@class='pages-current']/span");
            var nextUri = pageNav.SelectSingleNodeAttr(".//a[@rel='next']", "href");
            var regex = new Regex("p=(\\d+)");
            var getPageId = new Func<string, string>(uri => {
                if (uri == null) return null;
                var match = regex.Match(uri);
                return match.Success ? match.Groups[1].Value : null;
            });
            var page = new BookmarkUsersPage() {
                NextPageId = getPageId(nextUri),
                PageId = currentPageId,
                PrevPageId = getPageId(prevUri),
            };
            foreach (var divNode in nodes) {
                var node = divNode.SelectSingleNode("./a");
                var user = new User() {
                    Avatar = node.GetAttributeValue("data-profile_img", null),
                    Description = HttpUtility.HtmlDecode(divNode.InnerText),
                    Id = node.GetAttributeValue("data-user_id", null),
                    Name = node.GetAttributeValue("data-user_name", null),
                };
                page.Users.Add(user);
            }

            return page;
        }

        public string GetCookies()
        {
            return _client.GetCookies(BaseUri);
        }

        public void InjectCookies(string cookies)
        {
            _client.InjectCookies(cookies, ".pixiv.net");
        }

        public User GetMyStatus()
        {
            var indexUrl = BaseUri;
            var s = _client.GetString(_client.BuildRequest(indexUrl));
            return GetMyStatus(s);
        }

        public string GetRankListUrl(RankMode mode, string pageId)
        {
            
            return $"{BaseUri}/ranking.php?mode={mode.Stringify()}&p={pageId}&format=json";
        }

        public IllustrationsPage GetRankList(RankMode mode, string pageId)
        {
            if (mode.IsR18()) AssertLoggedIn();

            var url = GetRankListUrl(mode, pageId);
            var s = _client.GetString(_client.BuildRequest(url));
            var obj = JObject.Parse(s);
            if (obj == null)
                throw new Exception("解析排行榜信息时出错");
            
            var getPageId = new Func<string, string>(property => {
                var v = obj.Val<string>(property);
                if (v == "False") return null;
                return v;
            });
            var page = new IllustrationsPage() {
                NextPageId = getPageId("next"),
                PageId = pageId,
                PrevPageId = getPageId("prev"),
            };

            var contens = obj["contents"];
            foreach (var token in contens) {
                var illust = new Illustration() {
                    Bookmarked = token.Val<bool>("is_bookmarked"),
                    Count = token.Val<int>("illust_page_count"),
                    Height = token.Val<string>("height"),
                    Id = token.Val<string>("illust_id"),
                    Tags = token["tags"].AsList<string>(),
                    Thumbnail = token.Val<string>("url"),
                    Title = token.Val<string>("title"),
                    User = new User() {
                        Avatar = token.Val<string>("profile_img"),
                        Id = token.Val<string>("user_id"),
                        Name = token.Val<string>("user_name"),
                    },
                    ViewCount = token.Val<string>("view_count"),
                    Width = token.Val<string>("width"),
                };
                page.Illustrations.Add(illust);
            }

            return page;
        }

        private void AssertLoggedIn()
        {
            if (!isLoggedIn)
                throw new Exception("请先登录");
        }

        private User GetMyStatus(string s)
        {
            var any = "[\\s\\S]";
            var pattern = "pixiv.context.token = \"(.+?)\"";
            pattern += $"{any}+?pixiv.user.loggedIn = true";
            pattern += $"{any}+?pixiv.user.id = \"(.+?)\"";
            var match = Regex.Match(s, pattern);
            if (!match.Success)
                return null;
            token = match.Groups[1].Value;
            var userId = match.Groups[2].Value;

            isLoggedIn = true;

            return me = new User() {
                Id = userId
            };
        }

        

        
    }
}