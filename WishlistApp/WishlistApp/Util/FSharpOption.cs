using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WishlistApp.Util
{
    public static class FSharpOption
    {
        public static FSharpOption<T> None<T>()
        {
            return FSharpOption<T>.None;
        }

        public static FSharpOption<T> Some<T>(T value)
        {
            return FSharpOption<T>.Some(value);
        }
    }
}