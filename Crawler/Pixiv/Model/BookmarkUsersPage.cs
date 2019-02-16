using System.Collections.Generic;

namespace Crawler.Pixiv.Model
{
    public class BookmarkUsersPage : Page
    {
        public List<User> Users { get; set; } = new List<User>();
        
    }
}