using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WishlistApp.Models;

namespace WishlistApp.Util
{
    public delegate TActionResult ControllerActionHandler<TActionResult>(WishlistContext db, int userId) where TActionResult : ActionResult;
}