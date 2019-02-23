using System;
using System.Collections.Generic;

namespace Crawler.Pixiv.Model
{
    public class User
    {
        public string Avatar { get; set; }

        public string Description { get; set; }

        public bool Follow { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public List<Illustration> NewlyIllustrations { get; set; }

        public List<string> Tags { get; set; }
    }
}