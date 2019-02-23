using System.Collections.Generic;

namespace Crawler.Pixiv.Model
{
    public class CommentsPage
    {
        public List<Comment> Commnets { get; set; } = new List<Comment>();

        public bool HasNext { get; set; }

        public string Limit { get; set; }

        public string Offset { get; set; }
    }
}