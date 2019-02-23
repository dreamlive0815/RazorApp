namespace Crawler.Pixiv.Model
{
    public class UserIllustrationsPage : IllustrationsPage
    {
        public string PerPageCount { get; set; }

        public string TotalCount { get; set; }

        public string TotalPageCount { get; set; }
    }
}