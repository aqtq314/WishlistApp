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
    public class RollController : Controller
    {
        public ViewResult Index(int? userId = null)
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

        public JsonResult GetRollsFor(int userId)
        {
            return Context.New((db, currUserId) =>
            {
                var userName = DbHelper.GetUserName(userId, db);

                var hasFriendship = DbHelper.AreFriends(currUserId, userId, db);

                var rolls =
                    db.Rolls
                    .Where(r => r.UserId == userId)
                    .Where(
                        currUserId == userId ?
                        (Expression<Func<Roll, bool>>)(r => true) :
                        hasFriendship ?
                        (Expression<Func<Roll, bool>>)(r =>
                            r.Visibility == (byte)Visibility.Public || r.Visibility == (byte)Visibility.Friend) :
                        (Expression<Func<Roll, bool>>)(r =>
                            r.Visibility == (byte)Visibility.Public))
                    .OrderByDescending(r => r.TimeUtc)
                    .AsEnumerable()
                    .Select(r => new
                    {
                        r.RollId,
                        r.Content,
                        TimeUtc = r.TimeUtc.ToString(),
                        r.Visibility,
                        VisibilityString = ((Visibility)r.Visibility).ToString()
                    })
                    .ToList();

                return Json(new
                {
                    Success = true,
                    UserName = userName,
                    Rolls = rolls
                });
            });
        }

        public JsonResult AddRoll(RollEditModel model, byte visibility)
        {
            return Context.New((db, currUserId) =>
            {
                db.Rolls.Add(new Roll
                {
                    Content = model.Content,
                    UserId = currUserId,
                    Visibility = visibility,
                    TimeUtc = DateTime.UtcNow
                });

                db.SaveChanges();

                return Json(new { Success = true });
            });
        }

        public JsonResult ChangeRollVisibility(int rollId, byte visibility)
        {
            return Context.New((db, currUserId) =>
            {
                return db.Rolls
                    .FirstOrDefault(r => r.RollId == rollId)
                    .AsJsonComputation()
                    .NotNull()
                    .Authorized(roll => roll.UserId, currUserId)
                    .Out(roll =>
                    {
                        roll.Visibility = visibility;
                        db.SaveChanges();
                        return Json(new { Success = true });
                    });
            });
        }

        public JsonResult EditRoll(int rollId, RollEditModel model)
        {
            return Context.New((db, currUserId) =>
            {
                return db.Rolls
                    .FirstOrDefault(r => r.RollId == rollId)
                    .AsJsonComputation()
                    .NotNull()
                    .Authorized(roll => roll.UserId, currUserId)
                    .Out(roll =>
                    {
                        roll.Content = model.Content;
                        db.SaveChanges();
                        return Json(new { Success = true });
                    });
            });
        }

        public JsonResult RemoveRoll(int rollId)
        {
            return Context.New((db, currUserId) =>
            {
                return db.Rolls
                    .FirstOrDefault(r => r.RollId == rollId)
                    .AsJsonComputation()
                    .NotNull()
                    .Authorized(roll => roll.UserId, currUserId)
                    .Out(roll =>
                    {
                        db.Rolls.Remove(roll);
                        db.SaveChanges();

                        return Json(new { Success = true });
                    });
            });
        }
    }
}
