using System.Collections.Generic;

namespace Crawler.Girl.Model
{
    public class Album
    {
        public int Count { get; set; }

        public string Cover { get; set; }

        public string CreateTime { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// 点击量
        /// </summary>
        /// <value></value>
        public string Hits { get; set; }
        
        public string Id { get; set; }

        public string GirlId { get; set; }

        public string Tags { get; set; }
        
        public string Title { get; set; }
    }
}