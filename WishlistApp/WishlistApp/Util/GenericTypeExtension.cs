using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WishlistApp.Util
{
    public static class GenericTypeExtension
    {
        public static TResult Bind<T, TResult>(this T value, Func<T, TResult> binder)
        {
            return binder(value);
        }
    }
}