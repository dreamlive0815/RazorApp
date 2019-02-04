using System.Net;

namespace MyHttp
{
    public class FiddlerProxy : WebProxy
    {
        public FiddlerProxy() : base("localhost", 8888)
        {
        }

    }
}