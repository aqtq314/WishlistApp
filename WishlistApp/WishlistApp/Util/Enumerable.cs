using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WishlistApp.Util
{
    public static class Enumerable
    {
        public static IEnumerable<IGrouping<TKey, TElem>> GroupBy<T, TKey, TElem, TKeyProj>(
            this IEnumerable<T> source,
            Func<T, TKey> keySelector,
            Func<T, TElem> elementSelector,
            Func<TKey, TKeyProj> keyComparisonProjector)
        {
            return source.GroupBy(
                keySelector,
                elementSelector,
                EqualityComparer.FromProjection<TKey, TKeyProj>(keyComparisonProjector));
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static void ForEachIgnore<T, R>(this IEnumerable<T> source, Func<T, R> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }
    }
}