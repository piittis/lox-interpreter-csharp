using System.Collections.Generic;

namespace Lox
{
    static class DictionaryExtensions
    {
        public static void AddOrUpdate<T, U>(this Dictionary<T, U> dict, T key, U value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }
    }
}
