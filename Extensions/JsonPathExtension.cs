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