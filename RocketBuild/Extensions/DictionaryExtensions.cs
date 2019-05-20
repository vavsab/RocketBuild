using System.Collections.Generic;

namespace RocketBuild.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            TValue defaultValue = default(TValue))
            => !dictionary.TryGetValue(key, out var value) ? defaultValue : value;
    }
}
