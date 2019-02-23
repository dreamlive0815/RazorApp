using System.Collections.Generic;

namespace Extensions
{
    public static class ListExtension
    {
        public static List<T> SubList<T>(this List<T> list, int offset, int count)
        {
            var newL = new List<T>();
            if (offset >= list.Count) return newL;
            for(int i = 0, o = offset; i < count && o < list.Count; i++, o++) {
                newL.Add(list[o]);
            }
            return newL;
        }
    }
}