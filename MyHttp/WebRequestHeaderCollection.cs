using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace MyHttp
{
    public class WebRequestHeaderCollection : IRequestHeaderCollection
    {
        HttpWebRequest _owner;
        WebHeaderCollection _headers;

        public WebRequestHeaderCollection(HttpWebRequest owner, WebHeaderCollection headers)
        {
            _owner = owner;
            _headers = headers;
        }

        public override void Add(string key, string value)
        {
            if(Contains(key)) throw new ArgumentException($"键值{key}已存在");
            this[key] = value;
        }

        public override bool Contains(string key)
        {
            return _headers.AllKeys.Contains(key);
        }

        public override void Remove(string key)
        {
            _headers.Remove(key);
        }

        public override string this[string key]
        {
            get {
                return _headers[key] ?? null;
            }
            set {
                SetHeader(key, value);
            }
        }

        private void SetHeader(string key, string value)
        {
            _headers.Remove(key);
            if(RequestHeaders.Accept == key) {
                _owner.Accept = value;
            } else if(RequestHeaders.Referrer == key) {
                _owner.Referer = value;
            } else if(RequestHeaders.UserAgent == key) {
                _owner.UserAgent = value;
            } else {
                _headers.Add(key, value);
            }
        }

        public override Dictionary<string, string> AsDictionary()
        {
            var d = new Dictionary<string, string>();
            foreach(var header in _headers.AllKeys)
            {
                d.Add(header, _headers[header]);
            }
            return d;
        }
    }
}