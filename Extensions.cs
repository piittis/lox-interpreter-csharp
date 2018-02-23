using System.Collections.Generic;

namespace Lox
{
    static class Extensions
    {
        public static void AddOrUpdate<T, U>(this Dictionary<T, U> dict, T key, U value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }

        public static string Join(this IEnumerable<string> values, string seperator)
        {
            return string.Join(seperator, values);
        }
    }
}
