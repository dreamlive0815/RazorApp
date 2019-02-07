using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MyHttp
{
    public class DefaultRequestHeaderCollection : IRequestHeaderCollection
    {
        HttpClient _owner;
        HttpRequestHeaders _headers;

        public DefaultRequestHeaderCollection(HttpRequestHeaders headers) : this(null, headers)
        {
        }

        public DefaultRequestHeaderCollection(HttpClient owner, HttpRequestHeaders headers)
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
            return _headers.Contains(key);
        }

        public override void Remove(string key)
        {
            _headers.Remove(key);
        }

        public override string this[string key]
        {
            get {
                try {
                    return string.Join(";", _headers.GetValues(key));
                } catch {
                    return null;
                }
            }
            set {
                _headers.Remove(key);
                _headers.TryAddWithoutValidation(key, value);
            }
        }

        public override Dictionary<string, string> AsDictionary()
        {
            var d = new Dictionary<string, string>();
            foreach(var header in _headers)
            {
                d.Add(header.Key, string.Join(",", header.Value));
            }
            return d;
        }
    }

    public static class HttpRequestHeadersExtension
    {
        public static IRequestHeaderCollection AsIRequestHeaderCollection(this HttpRequestHeaders headers)
        {
            return new DefaultRequestHeaderCollection(headers);
        }
    }
}