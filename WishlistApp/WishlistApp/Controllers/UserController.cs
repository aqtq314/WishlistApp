using System;
using System.Collections.Generic;
using System.Linq;
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
    public class UserController : Controller
    {
        public ViewResult Index(int? userId)
        {
            return Context.New((db, currUserId) =>
            {
                int userIdNonNull = userId ?? currUserId;

                return View(new UserInfoJsonModel
                {
                    ID = userIdNonNull,
                    UserName = DbHelper.GetUserName(userIdNonNull, db),
                    IsSelf = currUserId == userIdNonNull
                });
            });
        }

        public JsonResult GetUserName(int userId)
        {
            return Context.New((db, _) =>
            {
                var userNameOption = DbHelper.GetUserNameOption(userId, db);

                return userNameOption == null ?
                    Json(new
                    {
                        Success = false,
                        Exception = "User does not exist."
                    }) :
                    Json(new
                    {
                        Success = true,
                        UserName = userNameOption.Value
                    });
            });
        }

        public JsonResult GetUserInfo(int userId)
        {
            return Context.New((db, currUserId) =>
            {
                var userProfile = db.UserProfiles.FirstOrDefault(up => up.UserId == userId);
                var membership = db.webpages_Membership.FirstOrDefault(m => m.UserId == userId);
                var friendshipCount = db.Friendships.Count(f => f.UserIdLower == userId || f.UserIdUpper == userId);
                var pictureCount = db.Pictures.Count(p => p.UserId == userId);
                var rollCount = db.Rolls.Count(r => r.UserId == userId);
                var wishlistCount = db.Wishlists.Count(wl => wl.UserId == userId);

                return Json(new
                {
                    Success = true,
                    UserName = userProfile.UserName,
                    CreationDate = membership.CreateDate.ToString(),
                    FriendshipCount = friendshipCount,
                    PictureCount = pictureCount,
                    RollCount = rollCount,
                    WishlistCount = wishlistCount
                });
            });
        }

        public ActionResult GetProfilePhoto(int userId)
        {
            return Context.New<ActionResult>((db, _) =>
            {
                var picture =
                    db.ProfilePictures
                    .Where(pp => pp.UserId == userId)
                    .Join(
                        db.Pictures,
                        pp => pp.PictureId,
                        p => p.PictureId,
                        (pp, p) => p.Thumbnail128)
                    .FirstOrDefault();

                if (picture == null)
                    return Redirect(@"~\Images\GenericProfilePicture.png");
                else
                    return File(picture, "image/jpeg");
            });
        }

        public ActionResult ChangeProfilePhoto(HttpPostedFileBase image)
        {
            return Context.New((db, userID) =>
            {
                try
                {
                    var img = new Image(image.InputStream);
                    var imgOrig = img.SerializeJpeg();
                    var img128 = img.Resize(128, 128).SerializeJpeg();

                    var picture = db.Pictures.Add(new Picture
                    {
                        UserId = userID,
                        Thumbnail128 = img128,
                        Content = imgOrig
                    });

                    var profilePicture =
                        db.ProfilePictures.FirstOrDefault(pp =>
                            pp.UserId == userID);
                    if (profilePicture == null)
                    {
                        db.ProfilePictures.Add(new ProfilePicture
                        {
                            UserId = userID,
                            Picture = picture
                        });
                    }
                    else
                    {
                        profilePicture.Picture = picture;
                    }

                    db.SaveChanges();

                    return Json(new { Success = true });
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        Success = false,
                        Exception = ex.Message
                    });
                }
            });
        }
    }
}
