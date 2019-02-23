namespace Crawler.Pixiv.Model
{
    public class Comment
    {
        public bool HasReplies { get; set; }

        public string Id { get; set; }

        public bool IsEmoji { get; set; }

        /// <summary>
        /// 父评论ID
        /// </summary>
        /// <value></value>
        public string ParentId { get ; set; }

        public User ReplyTo { get; set; }

        public string RootId { get; set; }

        public string Time { get; set; }

        public string Text { get; set; }

        public User User { get; set; }
    }
}