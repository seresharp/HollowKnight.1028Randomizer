using System.Collections.Generic;

namespace Randomizer.Util
{
    public static class DictionaryExtensions
    {
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> pair, out T1 item1, out T2 item2)
            => (item1, item2) = (pair.Key, pair.Value);

        public static void AddRange<T1, T2>(this Dictionary<T1, T2> dict, IEnumerable<KeyValuePair<T1, T2>> pairs)
        {
            foreach ((T1 key, T2 value) in pairs)
            {
                dict[key] = value;
            }
        }
    }
}
