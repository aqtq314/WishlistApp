using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using WishlistApp.Models;

namespace WishlistApp.Util
{
    public static class Context
    {
        public static TActionResult New<TActionResult>(ControllerActionHandler<TActionResult> controllerAction) where TActionResult : ActionResult
        {
            var userId = WebSecurity.CurrentUserId;

            using (var db = new WishlistContext())
            {
                return controllerAction(db, userId);
            }
        }
    }
}