using System.Net;

namespace MyHttp
{
    public class HttpClientInitializer
    {
        public static void Chrome(IHttpClient client)
        {
            var headers = client.Headers;
            headers.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            headers.AcceptEncoding = "gzip,deflate,br";
            headers.AcceptLanguage = "zh-CN,zh;q=0.9";
            headers.CacheControl = "no-cache";
            headers.Pragma = "no-cache";
            headers.UserAgent = UserAgents.Chrome;

            client.Decompression = DecompressionMethods.GZip;
        }
    }
}