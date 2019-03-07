
using System;
using System.Collections.Generic;
using System.Net;
using Request = System.Net.Http.HttpRequestMessage;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading.Tasks;

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


        public static int DefaultCommentsLimit { get; set; } = 20;

        /// <summary>
        /// 获取某一副插画详细信息时小图的默认质量
        /// </summary>
        /// <value></value>
        public ThumbnailQuality DefaultIllustInfoThumbnailQuality { get; set; } = ThumbnailQuality.Regular;

        /// <summary>
        /// 获取某副插画的推荐插画作品默认每页数量
        /// </summary>
        /// <value></value>
        public static int DefaultRecommendIllustsPerPageCount { get; set; } = 20;

        /// <summary>
        /// 获取用户插画作品默认每页数量
        /// </summary>
        /// <value></value>
        public static int DefaultUserIllustsPerPageCount { get; set; } = 20;

        /// <summary>
        /// API获取用户信息时最新插画作品的默认数量
        /// </summary>
        /// <value></value>
        public static int DefaultUsersProfileIllustNumber { get; set; } = 3;

        /// <summary>
        /// 获取用户资料时用户最新作品的默认图片质量
        /// </summary>
        /// <value></value>
        public static ThumbnailQuality DefaultUsersProfileIllustThumbnailQuality { get; set; } = ThumbnailQuality.S128;

        public static int DefaultUsersProfileNovelNumber { get; set; } = 3;

        public bool IsLoggedIn { get { return isLoggedIn; } }

        IHttpClient _client;
        bool isLoggedIn;
        string token;
        User me;

        public Pixiv(IHttpClient client)
        {
            _client = client;
            InitClient(_client);
        }

        private void InitClient(IHttpClient client)
        {
            HttpClientInitializer.Chrome(client);
            client.Headers.Referrer = BaseUri;
            client.ThrowExceptionWhenUnSuccessfulStatusCode = false;
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

        public string GetCommentsUrl(string illustId, int offset, int limit)
        {
            return $"{BaseUri}/ajax/illusts/comments/roots?illust_id={illustId}&offset={offset}&limit={limit}";
        }

        public CommentsPage GetComments(string illustId, int offset)
        {
            return GetComments(illustId, offset, DefaultCommentsLimit);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="illustId"></param>
        /// <param name="offset">从0开始</param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public CommentsPage GetComments(string illustId, int offset, int limit)
        {
            if (offset < 0) offset = 0;
            if (limit < 1) limit = 1;

            var url = GetCommentsUrl(illustId, offset, limit);
            var s = _client.GetString(_client.BuildRequest(url));

            var obj = GetJObject(s);
            if (obj == null)
                throw new Exception("解析插画评论集合信息时出错");
            AssertOKJsonResult(obj);

            var page = new CommentsPage() {
                HasNext = obj.JsonPathSingle("$.body").Val<bool>("hasNext"),
                Limit = limit.ToString(),
                Offset = offset.ToString(),
            };

            var comments = obj.JsonPath("$.body.comments[*]");
            foreach (var comment in comments) {
                var emojiId = comment.Val<string>("stampId");
                var replyToUserId = comment.Val<string>("replyToUserId");
                var c = new Comment() {
                    HasReplies = comment.Val<bool>("hasReplies"),
                    Id = comment.Val<string>("id"),
                    IsEmoji = emojiId != null,
                    ParentId = comment.Val<string>("commentParentId"),
                    RootId = comment.Val<string>("commentRootId"),
                    Time = comment.Val<string>("commentDate"),
                    Text = comment.Val<string>("comment"),
                    User = new User() {
                        Avatar = comment.Val<string>("img"),
                        Id = comment.Val<string>("userId"),
                        Name = comment.Val<string>("userName"),
                    },
                };
                if (replyToUserId != null) c.ReplyTo = new User() {
                    Id = replyToUserId,
                    Name = comment.Val<string>("replyToUserName"),
                };
                if (c.IsEmoji) c.Text = emojiId;
                page.Comments.Add(c);
            }

            return page;
        }

        public string GetBookmarkUsersNewIllustrationsUrl(string pageId)
        {
            return $"{BaseUri}/bookmark_new_illust.php?p={pageId}";
        }

        /// <summary>
        /// 所关注用户的最新作品
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public IllustrationsPage GetBookmarkUsersNewIllustrations(string pageId)
        {
            AssertLoggedIn();

            var url = GetBookmarkUsersNewIllustrationsUrl(pageId);
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
            var arr = GetJArray(items);

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

        public string GetBookmarkIllustrationsUrl(string pageId, bool showPublic = true)
        {
            var rest = showPublic ? "show" : "hide";
            return $"{BaseUri}/bookmark.php?rest={rest}&p={pageId}";
        }

        /// <summary>
        /// 获取收藏的作品 只有插画 只能获取到收藏数量
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="showPublic">true显示公开关注,false显示私人关注</param>
        /// <returns></returns>
        public BookmarkedIllustrationsPage GetBookmarkIllustrations(string pageId, bool showPublic = true)
        {
            AssertLoggedIn();

            var url = GetBookmarkIllustrationsUrl(pageId, showPublic);
            var s = _client.GetString(_client.BuildRequest(url));

            var doc = s.XPathSingle(".");
            var nodes = doc.SelectNodes(".//li[@class='image-item']");
            //TODO: 页码导航未验证过
            var pageNav = doc.SelectSingleNode(".//div[@class='_pager-complex']");
            var prevUri = pageNav.SelectSingleNodeAttr(".//a[@rel='prev']", "href");
            var currentPageId = pageNav.SelectSingleNodeHtml(".//li[@class='pages-current']/span");
            var nextUri = pageNav.SelectSingleNodeAttr(".//a[@rel='next']", "href");
            var regex = new Regex("p=(\\d+)");
            var getPageId = new Func<string, string>(uri => {
                if (uri == null) return null;
                var match = regex.Match(uri);
                return match.Success ? match.Groups[1].Value : "1";
            });
            
            var page = new BookmarkedIllustrationsPage() {
                NextPageId = getPageId(nextUri),
                PageId = currentPageId ?? "1",
                PrevPageId = getPageId(prevUri),

                IsPublic = showPublic,
            };

            foreach (var divNode in nodes) {
                var imgNode = divNode.SelectSingleNode(".//div[@class='_layout-thumbnail']/img");
                var userNode = divNode.SelectSingleNode(".//a[contains(@class, 'ui-profile-popup')]");
                var illust = new Illustration() {
                    BookmarkCount = divNode.SelectSingleNode(".//ul[@class='count-list']/li/a")?.InnerText,
                    BookmarkId = divNode.SelectSingleNodeAttr(".//input[@name='book_id[]']", "value"),
                    Id = imgNode.GetAttributeValue("data-id", null),
                    Tags = new List<string>(imgNode.GetAttributeValue("data-tags", "").Split(" ", StringSplitOptions.RemoveEmptyEntries)),
                    Thumbnail = imgNode.GetAttributeValue("data-src", null),
                    Title = divNode.SelectSingleNodeAttr(".//h1[@class='title']", "title"),
                    User = new User() {
                        Id = userNode.GetAttributeValue("data-user_id", null),
                        Name = userNode.GetAttributeValue("data-user_name", null),
                    },
                };
                page.Illustrations.Add(illust);
            }

            return page;
        }

        public string GetBookmarkUsersUrl(string pageId, bool showPublic = true)
        {
            var rest = showPublic ? "show" : "hide";
            return $"{BaseUri}/bookmark.php?type=user&rest={rest}&p={pageId}";
        }

        /// <summary>
        /// 获取我关注的用户
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="showPublic">true显示公开关注,false显示私人关注</param>
        /// <returns></returns>
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
                return match.Success ? match.Groups[1].Value : "1";
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
                    Follow = true,
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

        public string GetIllustrationInfoUrl(string illustId)
        {
            return $"{BaseUri}/member_illust.php?mode=medium&illust_id={illustId}";
        }

        public Illustration GetIllustrationInfo(string illustId)
        {
            AssertLoggedIn();

            var url = GetIllustrationInfoUrl(illustId);
            var s = _client.GetString(_client.BuildRequest(url));

            var any = "[\\s\\S]";
            var pattern = $"Object.freeze\\(arg\\){any}+?(\\{{token:.+?\\}})\\);<";
            var match = Regex.Match(s, pattern);
            if (!match.Success)
                throw new Exception("获取插画信息时出错");
            
            var obj = GetJObject(match.Groups[1].Value);
            var illust = obj.JsonPathSingle("$.preload.illust.*");
            var user = obj.JsonPathSingle("$.preload.user.*");
            if (obj == null || illust == null)
                throw new Exception("解析插画信息时出错");
        
            var desc = illust.Val<string>("description").Replace("<br />", Environment.NewLine);
            var node = desc.XPathSingle(".");
            var urls = illust["urls"];
            var i = new Illustration() {
                BookmarkCount = illust.Val<string>("bookmarkCount"),
                Bookmarked = illust["bookmarkData"].HasValues,
                Count = illust.Val<int>("pageCount"),
                Description = node.InnerText,
                Height = illust.Val<string>("height"),
                Id = illust.Val<string>("illustId"),
                LikeCount = illust.Val<string>("likeCount"),
                Liked = illust.Val<bool>("likeData"),
                Tags = illust.JsonPathValue<string>(".tags.tags[*].tag"),
                Thumbnail = urls.Val<string>(DefaultIllustInfoThumbnailQuality.Stringify()),
                Title = illust.Val<string>("illustTitle"),
                User = new User() {
                    Avatar = user.Val<string>("imageBig"),
                    Follow = user.Val<bool>("isFollowed"),
                    Id = user.Val<string>("userId"),
                    Name = user.Val<string>("name"),
                },
                ViewCount = illust.Val<string>("viewCount"),
                Width = illust.Val<string>("width"),
            };
            if (i.Bookmarked) i.BookmarkId = illust["bookmarkData"].Val<string>("id");
            if (i.Thumbnail == null)
                throw new Exception("无法获取插画的图片链接");
            var originUrl = urls.Val<string>(ThumbnailQuality.Original.Stringify());
            i.Date = Illustration.GetDateFromUrl(originUrl);
            var m = Regex.Match(originUrl, "\\.(\\w+?)$");
            if (!m.Success)
                throw new Exception("无法获取插画原图后缀");
            i.Suffix = m.Groups[1].Value;
            return i;
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

        /// <summary>
        /// 无法获取用户关注状态
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public IllustrationsPage GetRankList(RankMode mode, string pageId)
        {
            if (mode.IsR18()) AssertLoggedIn();

            var url = GetRankListUrl(mode, pageId);
            var s = _client.GetString(_client.BuildRequest(url));
            var obj = GetJObject(s);
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

        public string GetRecommendIllustrationIdsUrl(string illustId)
        {
            return $"{BaseUri}/ajax/illust/{illustId}/recommend/init?limit=1";
        }

        /// <summary>
        /// 获取一个作品的所有推荐作品ID
        /// </summary>
        /// <param name="illustId"></param>
        /// <returns></returns>
        public List<string> GetRecommendIllustrationIds(string illustId)
        {
            var url = GetRecommendIllustrationIdsUrl(illustId);
            var s = _client.GetString(_client.BuildRequest(url));

            var obj = GetJObject(s);
            if (obj == null)
                throw new Exception($"解析作品:{illustId}推荐作品ID集合时出错");
            AssertOKJsonResult(obj);

            var illustIds = obj.JsonPathValue<string>("$.body.illusts[*].workId");
            var nextIds = obj.JsonPathValue<string>("$.body.nextIds[*]");
            illustIds.AddRange(nextIds);
            
            return illustIds;
        }

        public string GetRecommendIllustrationsUrl(IEnumerable<string> illustIds)
        {
            var sb = new StringBuilder(); var f = true;
            var e = new Exception("需要有一个及以上的插画ID");
            if (illustIds == null)
                throw e;
            foreach (var id in illustIds) {
                if (f) f = !f; else sb.Append("&");
                sb.Append("illust_ids%5B%5D=");
                sb.Append(id);
            }
            if (f)
                throw e;
            return $"{BaseUri}/ajax/illust/recommend/illusts?{sb}";
        }

        public List<Illustration> GetRecommendIllustrations(IEnumerable<string> illustIds)
        {
            var list = new List<Illustration>();

            var url = GetRecommendIllustrationsUrl(illustIds);
            var s = _client.GetString(_client.BuildRequest(url));

            var obj = GetJObject(s);
            if (obj == null)
                throw new Exception($"解析推荐作品信息时出错");
            AssertOKJsonResult(obj);

            var illusts = obj.JsonPath("$.body.illusts[*]");
            foreach (var illust in illusts) {
                var i = new Illustration() {
                    Bookmarked = illust["bookmarkData"].HasValues,
                    Count = illust.Val<int>("illustPageCount"),
                    Id = illust.Val<string>("workId"),
                    Tags = illust["tags"].AsList<string>(),
                    Thumbnail = illust.Val<string>("imageUrl"),
                    Title = illust.Val<string>("title"),
                    User = new User() {
                        Avatar = illust.Val<string>("profileImageUrl"),
                        Id = illust.Val<string>("userId"),
                        Name = illust.Val<string>("userName"),
                    },
                };
                if (i.Bookmarked) i.BookmarkId = illust["bookmarkData"].Val<string>("id");
                list.Add(i);
            }

            return list;
        }

        public RecommendIllustrationsPage GetRecommendIllustrations(string illustId, int pageId)
        {
            return GetRecommendIllustrations(illustId, pageId, DefaultRecommendIllustsPerPageCount);
        }

        public RecommendIllustrationsPage GetRecommendIllustrations(string illustId, int pageId, int perPageCount)
        {
            var illustIds = GetRecommendIllustrationIds(illustId);
            var cnt = illustIds.Count;
            if (pageId < 1) pageId = 1;
            if (perPageCount < 1) perPageCount = DefaultRecommendIllustsPerPageCount;
            var getPageId = new Func<int, string>(id => {
                if (id < 1) return null;
                if ((id - 1) * perPageCount >= cnt) return null;
                return id.ToString();
            });
            var offset = (pageId - 1) * perPageCount;
            if (offset >= cnt) {
                pageId = (cnt + perPageCount - 1) / perPageCount;
                offset = (pageId - 1) * perPageCount;
            }
            var ids = illustIds.SubList(offset, perPageCount);
            var illusts = GetRecommendIllustrations(ids);

            var page = new RecommendIllustrationsPage() {
                Illustrations = illusts,
                NextPageId = getPageId(pageId + 1),
                PageId = pageId.ToString(),
                PerPageCount = perPageCount.ToString(),
                PrevPageId = getPageId(pageId - 1),
                TotalCount = cnt.ToString(),
                TotalPageCount = ((cnt + perPageCount -1) / perPageCount).ToString(),
            };

            return page;
        }

        public string GetUserIllustrationsUrl(string userId, IEnumerable<string> illustIds)
        {
            var sb = new StringBuilder(); var f = true;
            var e = new Exception("需要有一个及以上的插画ID");
            if (illustIds == null)
                throw e;
            foreach (var id in illustIds) {
                if (f) f = !f; else sb.Append("&");
                sb.Append("ids%5B%5D=");
                sb.Append(id);
            }
            if (f)
                throw e;
            return $"{BaseUri}/ajax/user/{userId}/profile/illusts?{sb}&is_manga_top=0";
        }

        public List<Illustration> GetUserIllustrations(string userId, IEnumerable<string> illustIds)
        {
            var url = GetUserIllustrationsUrl(userId, illustIds);
            var s = _client.GetString(_client.BuildRequest(url));

            var obj = GetJObject(s);
            if (obj == null)
                throw new Exception("解析用户插画集合信息时出错");
            AssertOKJsonResult(obj);

            var list = new List<Illustration>();
            var illusts = obj.JsonPath("$.body.works.*");
            foreach (var token in illusts) {
                var illust = new Illustration() {     
                    Bookmarked = token["bookmarkData"].HasValues,
                    Count = token.Val<int>("pageCount"),
                    Height = token.Val<string>("height"),
                    Id = token.Val<string>("id"),
                    Tags = token["tags"].AsList<string>(),
                    Title = token.Val<string>("title"),
                    Thumbnail = token.Val<string>("url"),
                    Width = token.Val<string>("width"),
                };
                if (illust.Bookmarked) illust.BookmarkId = token["bookmarkData"].Val<string>("id");
                list.Add(illust);
            }

            return list;
        }

        public UserIllustrationsPage GetUserIllustrations(string userId, int pageId)
        {
            return GetUserIllustrations(userId, pageId, DefaultUserIllustsPerPageCount);
        }

        public UserIllustrationsPage GetUserIllustrations(string userId, int pageId, int perPageCount)
        {
            var illustIds = GetUserIllustrationIds(userId);
            var cnt = illustIds.Count;
            if (pageId < 1) pageId = 1;
            if (perPageCount < 1) perPageCount = DefaultUserIllustsPerPageCount;
            var getPageId = new Func<int, string>(id => {
                if (id < 1) return null;
                if ((id - 1) * perPageCount >= cnt) return null;
                return id.ToString();
            });
            var offset = (pageId - 1) * perPageCount;
            if (offset >= cnt) {
                pageId = (cnt + perPageCount - 1) / perPageCount;
                offset = (pageId - 1) * perPageCount;
            }
            var ids = illustIds.SubList(offset, perPageCount);
            var illusts = GetUserIllustrations(userId, ids);

            var page = new UserIllustrationsPage() {
                Illustrations = illusts,
                NextPageId = getPageId(pageId + 1),
                PageId = pageId.ToString(),
                PerPageCount = perPageCount.ToString(),
                PrevPageId = getPageId(pageId - 1),
                TotalCount = cnt.ToString(),
                TotalPageCount = ((cnt + perPageCount -1) / perPageCount).ToString(),
            };

            return page;
        }

        public string GetUserIllustrationIdsUrl(string userId)
        {
            return $"{BaseUri}/ajax/user/{userId}/profile/all";
        }

        /// <summary>
        /// 获取用户所有插画的ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<string> GetUserIllustrationIds(string userId)
        {
            var url = GetUserIllustrationIdsUrl(userId);
            var s = _client.GetString(_client.BuildRequest(url));

            var obj = GetJObject(s);
            if (obj == null)
                throw new Exception("解析用户插画ID集合时出错");
            AssertOKJsonResult(obj);

            var illusts = obj.JsonPathKey("$.body.illusts");
            var manga = obj.JsonPathKey("$.body.manga");
            if (manga.Count > 0) {
                illusts.AddRange(manga);
            }
            //降序排序
            illusts.Sort((a, b) => -a.CompareTo(b));
            
            return illusts;
        }

        /// <summary>
        /// API方式,可获取最近发布的插画作品
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public string GetUsersProfileUrl(IEnumerable<string> userIds)
        {
            var idS = string.Join(",", userIds);
            return $"{BaseUri}/rpc/get_profile.php?user_ids={idS}&illust_num={DefaultUsersProfileIllustNumber}&novel_num={DefaultUsersProfileNovelNumber}";
        }

        public User GetUserProfile(string userId)
        {
            var dic = GetUsersProfile(new string[] { userId });
            if (!dic.ContainsKey(userId))
                throw new Exception("用户不存在");
            return dic[userId];
        }

        public Dictionary<string, User> GetUsersProfile(IEnumerable<string> userIds)
        {
            AssertLoggedIn();

            var dic = new Dictionary<string, User>();

            var url = GetUsersProfileUrl(userIds);
            var s = _client.GetString(_client.BuildRequest(url));

            var obj = GetJObject(s);
            if (obj == null)
                throw new Exception("解析用户信息时出错");
            AssertOKJsonResult(obj);
            var body = obj["body"];
            foreach (var token in body) {
                var user = new User {
                    Avatar = token.Val<string>("profile_img"),
                    Description = token.Val<string>("user_comment"),
                    Follow = token.Val<bool>("is_follow"),
                    Id = token.Val<string>("user_id"),
                    Name = token.Val<string>("user_name"),
                };
                var list = new List<Illustration>();
                var illusts = token["illusts"];
                foreach (var illust in illusts) {
                    var newI = new Illustration() {
                        Count = illust.Val<int>("illust_page_count"),
                        Height = illust.Val<string>("illust_height"),
                        Id = illust.Val<string>("illust_id"),
                        Suffix = illust.Val<string>("illust_ext"),
                        Tags = illust["tags"].AsList<string>(),
                        Title = illust.Val<string>("illust_title"),
                        Thumbnail = illust["url"].Val<string>(DefaultUsersProfileIllustThumbnailQuality.Stringify()),
                        Width = illust.Val<string>("illust_width"),
                    };
                    list.Add(newI);
                }
                user.NewlyIllustrations = list;
                dic.Add(user.Id, user);
            }

            return dic;
        }

        public Request GetBookmarkIllustrationRequest(string illustId)
        {
            var url = $"{BaseUri}/ajax/illusts/bookmarks/add";
            var request = _client.BuildRequest(url, JsonConvert.SerializeObject(new {
                comment = "",
                illust_id = illustId,
                restrict = 0,
                tags = new string[0],
            }));
            request.Headers.TryAddWithoutValidation("x-csrf-token", token);
            request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            return request;
        }

        public string GetModifyBookmarkIllustrationPageUrl(string illustId, bool isPublic = true)
        {
            var rest = isPublic ? "show" : "hide";
            return $"{BaseUri}/bookmark_add.php?illust_id={illustId}&rest={rest}&type=illust&p=1";
        }

        public string GetBookId(string illustId, bool isPublic = true)
        {
            var s = _client.GetString(_client.BuildRequest(GetModifyBookmarkIllustrationPageUrl(illustId, isPublic)));

            var id = s.XPathSingleAttr("//input[@name='book_id[]']", "value");
            return id;
        }

        public string BookmarkIllustration(string illustId)
        {
            AssertLoggedIn();

            var s = _client.GetString(GetBookmarkIllustrationRequest(illustId));

            var obj = GetJObject(s);
            if (obj == null)
                throw new Exception("解析关注用户操作的返回信息出错");
            AssertOKJsonResult(obj);

            return GetBookId(illustId);
        }

        public Request GetUnBookmarkIllustrationRequest(string bookId, bool isPublic = true)
        {
            var url = $"{BaseUri}/bookmark_setting.php";
            var rest = isPublic ? "show" : "hide";
            return _client.BuildRequest(url, new Dictionary<string, object>() {
                { "book_id%5B%5D", bookId },
                { "del", "1" },
                { "p", "1" },
                { "rest", rest },
                { "untagged", "0" },
                { "tt", token },
            });
        }

        public void UnBookmarkIllustration(string bookId, bool isPublic = true)
        {
            AssertLoggedIn();
            
            var s = _client.GetString(GetUnBookmarkIllustrationRequest(bookId, isPublic));

            var match = Regex.Match(s, "<p class=\"error-message\">(.+?)</p>");
            if (match.Success)
                throw new Exception(match.Groups[1].Value);
        }

        public Request GetCommentRequest(string illustId, string authorId, string text)
        {
            var url = $"{BaseUri}/rpc/post_comment.php";
            return _client.BuildRequest(url, new Dictionary<string, object>() {
                { "author_user_id", authorId },
                { "comment", text },
                { "illust_id", illustId },
                { "type", "comment" },
                { "tt", token },
            });
        }

        public Comment Comment(string illustId, string text)
        {
            return Comment(illustId, "", text);
        }

        /// <summary>
        /// 评论插画作品 只能评论文字
        /// </summary>
        /// <param name="illustId"></param>
        /// <param name="authorId">插画作者ID 可以为空字符串</param>
        /// <param name="text"></param>
        /// <returns>返回刚刚的评论</returns>
        public Comment Comment(string illustId, string authorId, string text)
        {
            AssertLoggedIn();
            
            var s = _client.GetString(GetCommentRequest(illustId, authorId, text));

            var obj = GetJObject(s);
            if (obj == null)
                throw new Exception("解析评论插画操作的返回信息出错");
            AssertOKJsonResult(obj);   

            var body = obj["body"];
            var c = new Comment() {
                Id = body.Val<string>("comment_id"),
                ParentId = body.Val<string>("parent_id"),
                Text = body.Val<string>("comment"),
                User = new User() {
                    Id = body.Val<string>("user_id"),
                    Name = body.Val<string>("user_name"),
                },
            };
            var stampId = body.Val<string>("stamp_id");
            if (stampId != null) {
                c.IsEmoji = true;
                c.Text = stampId;
            }

            return c;
        }
        
        public Request GetCommentEmojiRequest(string illustId, string authorId, string emojiId)
        {
            var url = $"{BaseUri}/rpc/post_comment.php";
            return _client.BuildRequest(url, new Dictionary<string, object>() {
                { "author_user_id", authorId },
                { "illust_id", illustId },
                { "stamp_id", emojiId },
                { "type", "stamp" },
                { "tt", token },
            });
        }

        public Comment CommentEmoji(string illustId, string emojiId)
        {
            return CommentEmoji(illustId, "", emojiId);
        }

        /// <summary>
        /// 评论插画 贴图表情
        /// </summary>
        /// <param name="illustId"></param>
        /// <param name="authorId">可以为空</param>
        /// <param name="emojiId"></param>
        /// <returns>返回刚刚的评论</returns>
        public Comment CommentEmoji(string illustId, string authorId, string emojiId)
        {
            AssertLoggedIn();
            
            var s = _client.GetString(GetCommentEmojiRequest(illustId, authorId, emojiId));

            var obj = GetJObject(s);
            if (obj == null)
                throw new Exception("解析评论插画(贴图表情)操作的返回信息出错");
            AssertOKJsonResult(obj);

            var body = obj["body"];
            var c = new Comment() {
                Id = body.Val<string>("comment_id"),
                ParentId = body.Val<string>("parent_id"),
                Text = body.Val<string>("comment"),
                User = new User() {
                    Id = body.Val<string>("user_id"),
                    Name = body.Val<string>("user_name"),
                },
            };
            var stampId = body.Val<string>("stamp_id");
            if (stampId != null) {
                c.IsEmoji = true;
                c.Text = stampId;
            }

            return c;
        }

        public Request GetRemoveMyCommentRequest(string illustId, string commentId)
        {
            var url = $"{BaseUri}/rpc_delete_comment.php";
            return _client.BuildRequest(url, new Dictionary<string, object>() {
                { "del_id", commentId },
                { "i_id", illustId },
                { "tt", token },
            });
        }

        /// <summary>
        /// 删除自己的评论 不论成功与非都不会抛错
        /// </summary>
        /// <param name="illustId"></param>
        /// <param name="commentId"></param>
        public void RemoveMyComment(string illustId, string commentId)
        {
            AssertLoggedIn();
            
            var s = _client.GetString(GetRemoveMyCommentRequest(illustId, commentId));

            var obj = GetJObject(s);
            if (obj == null)
                throw new Exception("解析删除评论操作的返回信息出错");
            AssertOKJsonResult(obj);
        }

        public Request GetFollowUserRequest(string userId)
        {
            var url = $"{BaseUri}/bookmark_add.php";
            return _client.BuildRequest(url, new Dictionary<string, object>() {
                { "format", "json" },
                { "mode", "add" },
                { "restrict", "0" },
                { "tag", "" },
                { "type", "user" },
                { "user_id", userId },
                { "tt", token },
            });
        }

        public void FollowUser(string userId)
        {
            AssertLoggedIn();
            
            var s = _client.GetString(GetFollowUserRequest(userId));

            var match = Regex.Match(s, "<p class=\"error-message\">(.+?)</p>");
            if (match.Success)
                throw new Exception(match.Groups[1].Value);
        }

        public Request GetUnFollowUserRequest(string userId)
        {
            var url = $"{BaseUri}/rpc_group_setting.php";
            return _client.BuildRequest(url, new Dictionary<string, object>() {
                { "mode", "del" },
                { "id", userId },
                { "type", "bookuser" },
                { "tt", token },
            });
        }

        /// <summary>
        /// 无论ID是否存在都不会抛错
        /// </summary>
        /// <param name="userId"></param>
        public void UnFollowUser(string userId)
        {
            AssertLoggedIn();
            
            var s = _client.GetString(GetUnFollowUserRequest(userId));

        }

        public Request GetLikeIllustrationRequest(string illustId)
        {
            var url = $"{BaseUri}/ajax/illusts/like";
            var request = _client.BuildRequest(url, JsonConvert.SerializeObject(new {
                illust_id = illustId,
            }));
            request.Headers.TryAddWithoutValidation("x-csrf-token", token);
            request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            return request;
        }

        public void LikeIllustration(string illustId)
        {
            AssertLoggedIn();
            
            var s = _client.GetString(GetLikeIllustrationRequest(illustId));

            var obj = GetJObject(s);
            if (obj == null)
                throw new Exception("解析点赞插画操作的返回信息出错");
            AssertOKJsonResult(obj);            
        }

        public void InjectCookies(string cookies)
        {
            _client.InjectCookies(cookies, ".pixiv.net");
        }

        private void AssertLoggedIn()
        {
            if (!isLoggedIn)
                throw new Exception("请先登录");
        }

        private void AssertOKJsonResult(JToken token)
        {
            var errorT = token["error"];
            if (errorT == null) return;
            var error = errorT.Value<bool>();
            if (error) {
                var message = token.Val<string>("message") ?? "Default Exception Message";
                throw new Exception(message);
            }
        }

        private JArray GetJArray(string s)
        {
            try {
                return JArray.Parse(s);
            } catch {
                return null;
            }
        }

        private JObject GetJObject(string s)
        {
            try {
                return JObject.Parse(s);
            } catch {
                return null;
            }
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