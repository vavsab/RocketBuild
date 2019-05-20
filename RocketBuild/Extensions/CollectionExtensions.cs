using System.Collections.Generic;

namespace RocketBuild.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> sequence)
        {
            foreach (T item in sequence)
            {
                collection.Add(item);
            }
        }
    }
}