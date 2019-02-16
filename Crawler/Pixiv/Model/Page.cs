namespace Crawler.Pixiv.Model
{
    public class Page
    {
        public bool HasNextPage { get { return !string.IsNullOrEmpty(NextPageId); } }

        public bool HasPrevPage { get { return !string.IsNullOrEmpty(PrevPageId); } }

        public string NextPageId { get; set; }

        public string PageId { get; set; }

        public string PrevPageId { get; set; }
    }
}