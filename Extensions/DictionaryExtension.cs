using System.Collections.Generic;

namespace Extensions
{
    public static class DictionaryExtension
    {
        public static void AddRange<T1, T2>(this Dictionary<T1, T2> dic1, Dictionary<T1, T2> dic2)
        {
            foreach(var pair in dic2)
            {
                dic1[pair.Key] = pair.Value;
            }
        }
    }
}