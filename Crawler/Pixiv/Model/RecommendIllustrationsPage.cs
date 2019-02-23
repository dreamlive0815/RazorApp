namespace Crawler.Pixiv.Model
{
    public class RecommendIllustrationsPage : IllustrationsPage
    {
        public string PerPageCount { get; set; }

        public string TotalCount { get; set; }

        public string TotalPageCount { get; set; }
    }
}