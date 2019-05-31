using System;
using System.Collections.Generic;

namespace Tools.Core
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> DistinctBy<T,V>(this IEnumerable<T> enumerable, Func<T,V> f)
        {
            var hashSet = new HashSet<V>();
            foreach (var e in enumerable)
            {
                var v = f(e);
                if (hashSet.Add(v))
                    yield return e;
            }
        }
    }
}