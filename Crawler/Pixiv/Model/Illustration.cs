using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Crawler.Pixiv.Model
{
    public class Illustration
    {
        private static Regex regex = new Regex("/img/(\\d{4}/\\d{2}/\\d{2}/\\d{2}/\\d{2}/\\d{2})/");

        public static string GetDateFromUrl(string url)
        {
            var match = regex.Match(url);
            return match.Success ? match.Groups[1].Value : null;
        }

        public bool Bookmarked { get; set; }

        public int Count { get; set; }

        public string Date { get; set; }

        public string Height { get; set; }

        public string Id { get; set; }

        /// <summary>
        /// 图片后缀
        /// </summary>
        /// <value></value>
        public string Suffix { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public string Thumbnail { get; set; }

        public string Title { get; set; }

        public User User { get; set; }

        public string ViewCount { get; set; }
        
        public string Width { get; set; }
    }
}