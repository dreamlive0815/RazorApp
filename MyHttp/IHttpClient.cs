using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Extensions;

namespace MyHttp
{
    public abstract class IHttpClient : IDisposable
    {
        public abstract Uri BaseAddress { get; set; }

        public abstract CookieContainer Cookies { get; }

        public abstract DecompressionMethods Decompression { get; set; }

        public abstract IRequestHeaderCollection Headers { get; }

        public abstract Stream GetStream(string requestUri);

        protected abstract HttpResponseMessage SendProto(HttpRequestMessage request);

        protected abstract Task<HttpResponseMessage> SendAsyncProto(HttpRequestMessage request);

        public abstract void Dispose();

        public event Action<IHttpClient, HttpRequestMessage> BeforeSendRequest;

        public event Action<IHttpClient, Exception> OnError;

        /// <summary>
        /// 超时 单位（ms）
        /// </summary>
        /// <value></value>
        public abstract int Timeout { get; set; }

        public abstract IWebProxy Proxy { get; set; }

        /// <summary>
        /// 设置POST时默认的Content-Type
        /// </summary>
        /// <value></value>
        public string DefaultContentType { get; set; } = "application/x-www-form-urlencoded";

        public bool ThrowExceptionWhenUnSuccessfulStatusCode { get; set; } = true;

        public static string BuildQueryString(Dictionary<string, object> formParams)
        {
            if(formParams == null) return "";
            var sb = new StringBuilder();
            var f = true;
            foreach (var pair in formParams)
            {
                if (f) f = false; else sb.Append('&');
                sb.Append(pair.Key);
                sb.Append('=');
                sb.Append(HttpUtility.UrlEncode($"{pair.Value}"));
            }
            return sb.ToString();
        }

        public static string GetUrlFileName(string url)
        {
            url = Regex.Replace(url, "\\?.+$", "");
            var match = new Regex("[^/]+$").Match(url);
            if(!match.Success) return "untitled";
            return match.Value;
        }

        public void AddCookie(Cookie cookie)
        {
            Cookies.Add(cookie);
        }

        public void AddCookie(string name, string value, string domain, string path = "/")
        {
            Cookies.Add(new Cookie(name, value, path, domain));
        }

        /// <summary>
        /// GET
        /// </summary>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        public HttpRequestMessage BuildRequest(string requestUri)
        {
            return new HttpRequestMessage(HttpMethod.Get, requestUri);
        }

        /// <summary>
        /// POST
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public HttpRequestMessage BuildRequest(string requestUri, string body)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            var content = new StringContent(body);
            if(DefaultContentType != null) content.Headers.ContentType = MediaTypeHeaderValue.Parse(DefaultContentType);
            request.Content = content;
            return request;
        }

        /// <summary>
        /// POST
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="postParams"></param>
        /// <returns></returns>
        public HttpRequestMessage BuildRequest(string requestUri, Dictionary<string, object> postParams)
        {
            return BuildRequest(requestUri, BuildQueryString(postParams));
        }

        public void DownloadFile(HttpRequestMessage request, string filePath)
        {
            BeforeSendRequest?.Invoke(this, request);
            try {
                var webReq = WebRequest.Create(request.RequestUri.OriginalString) as HttpWebRequest;
                webReq.CookieContainer = Cookies;
                webReq.Proxy = Proxy;
                var c = new WebRequestHeaderCollection(webReq, webReq.Headers);
                var d = GetMergedHeaderDictionary(request);
                c.AddRange(d);

                if(request.Method == HttpMethod.Post && request.Content != null)
                {
                    webReq.Method = "POST";
                    webReq.ContentType = DefaultContentType;
                    var t = request.Content.ReadAsStreamAsync();
                    t.Wait();
                    using(var reqS = t.Result)
                    using(var webReqS = webReq.GetRequestStream())
                    {
                        reqS.CopyTo(webReqS);
                    }
                }

                using(var resp = webReq.GetResponse())
                using(var respS = resp.GetResponseStream())
                using(var fS = new FileStream(filePath, FileMode.Create))
                {
                    respS.CopyTo(fS);
                }
            } catch(Exception e) {
                OnError?.Invoke(this, e);
                throw e;
            }
        }

        public Task DownloadFileAsync(HttpRequestMessage request, string filePath)
        {
            return Task.Run(() => {
                DownloadFile(request, filePath);
            });
        }

        public string GetCookies(string uri)
        {
            var cookies = Cookies.GetCookies(new Uri(uri));
            var sb = new StringBuilder();
            for (var i = 0; i < cookies.Count; i++) {
                var cookie = cookies[i];
                if (i > 0) sb.Append(";");
                sb.Append(cookie.Name); sb.Append("="); sb.Append(cookie.Value);
            }
            return sb.ToString();
        }

        public Dictionary<string, string> GetMergedHeaderDictionary(HttpRequestMessage request)
        {
            var d1 = Headers.AsDictionary();
            var d2 = request.Headers.AsIRequestHeaderCollection().AsDictionary();
            d1.AddRange(d2);
            return d1;
        }

        public virtual string GetString(HttpRequestMessage request)
        {
            try {
                var response = Send(request);
                var readTask = response.Content.ReadAsStringAsync();
                return readTask.Result;
            } catch(Exception e) {
                OnError?.Invoke(this, e);
                throw e;
            }
        }

        public virtual Task<string> GetStringAsync(HttpRequestMessage request)
        {
            return SendAsync(request).ContinueWith<string>(task => {
                AssertSuccess<HttpResponseMessage>(task);
                var readTask = task.Result.Content.ReadAsStringAsync();
                try {
                    readTask.Wait();
                    return readTask.Result;
                } catch(Exception e) {
                    OnError?.Invoke(this, e);
                    throw e;
                }
            });
        }

        public void InjectCookies(string cookieStr, string domain, string path = "/")
        {
            if(cookieStr == null) return;
            foreach(var cookie in cookieStr.Split(';'))
            {
                var c = cookie.Trim();
                var pair = c.Split('=');
                Cookies.Add(new Cookie(pair[0], pair.Length > 1 ? pair[1] : "", path, domain));
            }
        }

        public HttpResponseMessage Send(HttpRequestMessage request)
        {
            BeforeSendRequest?.Invoke(this, request);
            var response = SendProto(request);
            if (!response.IsSuccessStatusCode && ThrowExceptionWhenUnSuccessfulStatusCode) {
                var e = new Exception(response.ReasonPhrase);
                OnError?.Invoke(this, e);
                throw e;
            }
            return response;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            BeforeSendRequest?.Invoke(this, request);
            if (ThrowExceptionWhenUnSuccessfulStatusCode) {
                return Task.Run<HttpResponseMessage>(() => {
                    var t = SendAsyncProto(request);
                    t.Wait();
                    var response = t.Result;
                    if (!response.IsSuccessStatusCode) {
                        var e = new Exception(response.ReasonPhrase);
                        OnError?.Invoke(this, e);
                        throw e;
                    }
                    return response;
                });
            }
            return SendAsyncProto(request);
        }

        public void SetWebProxy(string host, int port)
        {
            Proxy = new WebProxy(host, port);
        }

        private void AssertSuccess(Task task)
        {
            if(task.IsFaulted)
            {
                OnError?.Invoke(this, task.Exception);
                throw task.Exception;
            }
        }

        private void AssertSuccess<T>(Task<T> task)
        {
            if(task.IsFaulted)
            {
                OnError?.Invoke(this, task.Exception);
                throw task.Exception;
            }
        }
    }
}