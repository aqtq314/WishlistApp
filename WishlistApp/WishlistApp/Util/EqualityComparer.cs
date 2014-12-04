using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WishlistApp.Util
{
    public static class EqualityComparer
    {
        private class ProjectionEqualityComparer<T, TProj> : IEqualityComparer<T>
        {
            Func<T, TProj> projection;
            IEqualityComparer<TProj> comparer;

            public ProjectionEqualityComparer(Func<T, TProj> _projection)
            {
                if (_projection == null)
                    throw new ArgumentNullException("projection");

                projection = _projection;
                comparer = EqualityComparer<TProj>.Default;
            }

            public bool Equals(T x, T y)
            {
                return comparer.Equals(projection(x), projection(y));
            }

            public int GetHashCode(T obj)
            {
                return comparer.GetHashCode(projection(obj));
            }
        }

        public static IEqualityComparer<T> FromProjection<T, TProj>(Func<T, TProj> projection)
        {
            return new ProjectionEqualityComparer<T, TProj>(projection);
        }
    }
}