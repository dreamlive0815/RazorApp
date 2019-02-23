using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extensions
{
    public static class StringJsonPathExtension
    {
        public static JToken JsonPathSingle(this string s, string jsonPath)
        {
            if (s == null) return null;
            var token = JToken.Parse(s);
            return token?.SelectToken(jsonPath);
        }

        public static IEnumerable<JToken> JsonPath(this string s, string jsonPath)
        {
            if (s == null) return new JToken[0];
            var token = JToken.Parse(s);
            return token?.SelectTokens(jsonPath);
        }
    }

    public static class JTokenJsonPathExtension
    {

        public static JToken JsonPathSingle(this JToken token, string jsonPath)
        {
            if (token == null) return null;
            return token.SelectToken(jsonPath);
        }

        public static IEnumerable<JToken> JsonPath(this JToken token, string jsonPath)
        {
            if (token == null) return new JToken[0];
            return token.SelectTokens(jsonPath);
        }

        public static List<string> JsonPathKey(this JToken token, string jsonPath)
        {
            var tokens = JsonPathSingle(token, jsonPath);
            var list = new List<string>();
            if (tokens == null) return list;
            foreach (var tok in tokens) {
                var prop = tok as JProperty;
                if (prop == null) continue;
                list.Add(prop.Name);
            }
            return list;
        }

        public static List<T> JsonPathValue<T>(this JToken token, string jsonPath)
        {
            var tokens = JsonPath(token, jsonPath);
            var list = new List<T>();
            if (tokens == null) return list;
            foreach (var tok in tokens) {
                list.Add(tok.Value<T>());
            }
            return list;
        }

        public static List<T> AsList<T>(this JToken token)
        {
            var list = new List<T>();
            if (token == null) return list;
            foreach (var child in token) {
                list.Add(child.Value<T>());
            }
            return list;
        }

        public static T Val<T>(this JToken token, string property)
        {
            if (token == null) return default(T);
            var childToken = token[property];
            if (childToken == null) return default(T);
            return childToken.Value<T>();
        }
    }
}