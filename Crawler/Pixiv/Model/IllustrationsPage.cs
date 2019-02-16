using System;
using System.Collections.Generic;

namespace Crawler.Pixiv.Model
{
    public class IllustrationsPage : Page
    {
        public List<Illustration> Illustrations { get; set; } = new List<Illustration>();
        
    }
}