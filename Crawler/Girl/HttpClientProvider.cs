
using MyHttp;

namespace Crawler.Girl
{
    public class HttpClientProvider
    {
        public static IHttpClient Provide()
        {
            return new MyHttpClient();
        }
    }
}