using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Crawler.Pixiv.Model
{
    public class Illustration
    {

        public static string ImageBaseUri { get { return "https://i.pximg.net"; } }

        private static Regex regex = new Regex("/img/(\\d{4}/\\d{2}/\\d{2}/\\d{2}/\\d{2}/\\d{2})/");

        public static string GetDateFromUrl(string url)
        {
            var match = regex.Match(url);
            return match.Success ? match.Groups[1].Value : null;
        }

        public string BookmarkCount { get; set; }

        public bool Bookmarked { get; set; }

        public string BookmarkId { get; set; }

        public int Count { get; set; }

        public string Date { get; set; }

        public string Description { get; set; }

        public string Height { get; set; }

        public string Id { get; set; }

        public string LikeCount { get; set; }

        /// <summary>
        /// 只有插画详细信息里能获取
        /// </summary>
        /// <value></value>
        public bool Liked { get; set; }

        /// <summary>
        /// 图片后缀
        /// </summary>
        /// <value></value>
        public string Suffix { get; set; }

        public List<string> Tags { get; set; }

        public string Thumbnail { get; set; }

        public string Title { get; set; }

        public User User { get; set; }

        public string ViewCount { get; set; }
        
        public string Width { get; set; }
        
        public string GetImageUrl(ThumbnailQuality quality, int index = 0)
        {
            if (Date == null)
                throw new Exception("插画日期为空时无法获取插画图片链接");
            //除了原图外所有图片默认后缀统一为jpg
            var ext = "jpg";
            if (quality == ThumbnailQuality.Original)
            {
                if (Suffix == null)
                    throw new Exception("后缀为空时无法获取原图链接");
                ext = Suffix;
            }
            switch (quality)
            {
                case ThumbnailQuality.Original: return  $"{ImageBaseUri}/img-original/img/{Date}/{Id}_p{index}.{ext}";
                //original
                default: return $"{ImageBaseUri}/img-original/img/{Date}/{Id}_p{index}.{ext}";
            }
        }

        public List<string> GetOriginalImageUrls()
        {
            var list = new List<string>();
            for(var i = 0; i < Count; i++) {
                list.Add(GetImageUrl(ThumbnailQuality.Original, i));
            }
            return list;
        }
    }
}