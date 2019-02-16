
using MyHttp;

namespace Crawler.Pixiv
{
    public class HttpClientProvider
    {
        public static IHttpClient Provide()
        {
            return new MyHttpClient();
        }
    }
}