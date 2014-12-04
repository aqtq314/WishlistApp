using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using WishlistApp.Filters;
using WishlistApp.Lib;
using WishlistApp.Models;
using WishlistApp.Util;

namespace WishlistApp.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class WishlistController : Controller
    {
        public ActionResult Index(int? userId)
        {
            return Context.New((db, currUserId) =>
            {
                int userIdNonNull = userId ?? currUserId;

                var userModel = new UserInfoJsonModel
                {
                    ID = userIdNonNull,
                    UserName = DbHelper.GetUserName(userIdNonNull, db),
                    IsSelf = currUserId == userIdNonNull
                };

                return View(userModel);
            });
        }

        public ActionResult GetWishlistsFor(int userId)
        {
            return Context.New((db, currUserId) =>
            {
                var userName = DbHelper.GetUserName(userId, db);

                var areFriends = DbHelper.AreFriends(currUserId, userId, db);

                var wishlists =
                    db.Wishlists
                    .Where(wl => wl.UserId == userId)
                    .Where(
                        userId == currUserId ?
                        (Expression<Func<Wishlist, bool>>)(wl => true) :
                        areFriends ?
                        (Expression<Func<Wishlist, bool>>)(wl =>
                            wl.Visibility == (byte)Visibility.Public || wl.Visibility == (byte)Visibility.Friend) :
                        (Expression<Func<Wishlist, bool>>)(wl =>
                            wl.Visibility == (byte)Visibility.Public))
                    .GroupJoin(
                        db.WishlistItems,
                        wl => wl.WishlistId,
                        wli => wli.WishlistId,
                        (wl, wli) => new { wl.WishlistId, wl.Title, wl.TimeUtc, wl.Visibility, WishlistItems = wli })
                    .AsEnumerable()
                    .OrderByDescending(wl => wl.TimeUtc)
                    .Select(g => new
                    {
                        g.WishlistId,
                        g.Title,
                        g.Visibility,
                        VisibilityString = ((Visibility)g.Visibility).ToString(),
                        TimeUtc = g.TimeUtc.ToString(),
                        WishlistItems =
                            g.WishlistItems.Select(wli => new
                            {
                                wli.WishlistItemId,
                                wli.Content
                            })
                            .ToList()
                    })
                    .ToList();

                return Json(new
                {
                    Success = true,
                    UserName = userName,
                    Wishlists = wishlists
                });
            });
        }

        public ActionResult AddWishlist(WishlistEditModel model, byte visibility)
        {
            return Context.New((db, currUserId) =>
            {
                var wl = db.Wishlists.Add(new Wishlist
                {
                    UserId = currUserId,
                    Title = model.Title,
                    Visibility = visibility,
                    TimeUtc = DateTime.UtcNow
                });

                if (model.WishlistItems == null)
                    model.WishlistItems = new WishlistEditModel.WishlistItemEditModel[] { };

                model.WishlistItems
                .Select(wli => new WishlistItem
                {
                    Wishlist = wl,
                    Content = wli.Content
                })
                .ForEachIgnore(db.WishlistItems.Add);

                db.SaveChanges();

                return Json(new { Success = true });
            });
        }

        public ActionResult ChangeWishlistVisibility(int wishlistId, byte visibility)
        {
            return Context.New((db, currUserId) =>
            {
                return db.Wishlists
                    .FirstOrDefault(wl => wl.WishlistId == wishlistId)
                    .AsJsonComputation()
                    .NotNull()
                    .Authorized(wl => wl.UserId, currUserId)
                    .Out(wl =>
                    {
                        wl.Visibility = visibility;
                        db.SaveChanges();
                        return Json(new { Success = true });
                    });
            });
        }

        public ActionResult EditWishlist(int wishlistId, WishlistEditModel model)
        {
            return Context.New((db, currUserId) =>
            {
                return db.Wishlists
                    .FirstOrDefault(wl2 => wl2.WishlistId == wishlistId)
                    .AsJsonComputation()
                    .NotNull()
                    .Authorized(wl => wl.UserId, currUserId)
                    .Out(wl =>
                    {
                        wl.Title = model.Title;

                        wl.WishlistItems.ForEachIgnore(
                            db.WishlistItems.Remove);

                        model.WishlistItems
                            .Select(wli => new WishlistItem
                            {
                                Content = wli.Content,
                                Wishlist = wl
                            })
                            .ForEachIgnore(db.WishlistItems.Add);

                        db.SaveChanges();

                        return Json(new { Success = true });
                    });
            });
        }

        public ActionResult RemoveWishlist(int wishlistId)
        {
            return Context.New((db, currUserId) =>
            {
                return db.Wishlists
                    .FirstOrDefault(wl2 => wl2.WishlistId == wishlistId)
                    .AsJsonComputation()
                    .NotNull()
                    .Authorized(wl => wl.UserId, currUserId)
                    .Out(wl =>
                    {
                        wl.WishlistItems.ToList().ForEachIgnore(
                            db.WishlistItems.Remove);
                        db.Wishlists.Remove(wl);
                        db.SaveChanges();

                        return Json(new { Success = true });
                    });
            });
        }
    }
}
