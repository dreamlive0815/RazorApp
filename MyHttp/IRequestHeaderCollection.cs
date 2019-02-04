using System.Collections;
using System.Collections.Generic;

namespace MyHttp
{
    public abstract class IRequestHeaderCollection : IEnumerable<KeyValuePair<string, string>>
    {
        public abstract void Add(string key, string value);

        public abstract bool Contains(string key);

        public abstract void Remove(string key);

        public abstract string this[string key] { get; set; }

        public abstract Dictionary<string, string> AsDictionary();

        public string Accept
        {
            get { return this[RequestHeaders.Accept]; }
            set { this[RequestHeaders.Accept] = value; }
        }

        public string AcceptEncoding
        {
            get { return this[RequestHeaders.AcceptEncoding]; }
            set { this[RequestHeaders.AcceptEncoding] = value; }
        }

        public string AcceptLanguage
        {
            get { return this[RequestHeaders.AcceptLanguage]; }
            set { this[RequestHeaders.AcceptLanguage] = value; }
        }

        public string CacheControl
        {
            get { return this[RequestHeaders.CacheControl]; }
            set { this[RequestHeaders.CacheControl] = value; }
        }

        public string Pragma
        {
            get { return this[RequestHeaders.Pragma]; }
            set { this[RequestHeaders.Pragma] = value; }
        }

        public string Referrer
        {
            get { return this[RequestHeaders.Referrer]; }
            set { this[RequestHeaders.Referrer] = value; }
        }

        public string UserAgent
        {
            get { return this[RequestHeaders.UserAgent]; }
            set { this[RequestHeaders.UserAgent] = value; }
        }

        public void AddRange(Dictionary<string, string> d)
        {
            foreach(var pair in d)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return AsDictionary().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}