using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WishlistApp.Lib;

namespace WishlistApp.Util
{
    public static class Validation
    {
        private static JsonResult Json<T>(T data)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = null,
                ContentEncoding = null,
                JsonRequestBehavior = JsonRequestBehavior.DenyGet
            };
        }

        public static JsonResult JsonFail(string exMessage)
        {
            return Json(new
            {
                Success = false,
                Exception = exMessage
            });
        }

        public static computation<TIn, JsonResult> AsJsonComputation<TIn>(this TIn value)
        {
            return value.AsComputation<TIn, JsonResult>();
        }

        public static computation<TIn, JsonResult> In<TIn>(TIn value)
        {
            return value.AsComputation<TIn, JsonResult>();
        }

        public static computation<TIn, TFallback> NotNull<TIn, TFallback>(this computation<TIn, TFallback> comp, TFallback nullFallback)
        {
            return comp.Try(
                item =>
                    item == null ?
                    FSharpOption<TFallback>.Some(nullFallback) :
                    FSharpOption<TFallback>.None);
        }

        public static computation<TIn, JsonResult> NotNull<TIn>(this computation<TIn, JsonResult> comp)
        {
            return comp.Try(
                item =>
                    item == null ?
                    FSharpOption<JsonResult>.Some(JsonFail("Item does not exist.")) :
                    FSharpOption<JsonResult>.None);
        }

        public static computation<TIn, JsonResult> Authorized<TIn>(this computation<TIn, JsonResult> comp, Func<TIn, int> getEntityUserId, int userId)
        {
            return comp.Try(
                item =>
                    getEntityUserId(item) == userId ?
                    FSharpOption<JsonResult>.None :
                    FSharpOption<JsonResult>.Some(JsonFail("Unauthorized request.")));
        }
    }
}