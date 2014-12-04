using Microsoft.FSharp.Core;
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

namespace WishlistApp.Util
{
    public static class DbHelper
    {
        public static bool AreFriends(int userId, int userId_2, WishlistContext db)
        {
            var userIdLower = Math.Min(userId, userId_2);
            var userIdUpper = Math.Max(userId, userId_2);

            return db.Friendships
                .Any(fs => fs.UserIdLower == userIdLower && fs.UserIdUpper == userIdUpper);
        }

        public static FSharpOption<string> GetUserNameOption(int userId, WishlistContext db)
        {
            var userProfile = db.UserProfiles.FirstOrDefault(u => u.UserId == userId);

            return userProfile == null ?
                FSharpOption<string>.None :
                FSharpOption<string>.Some(userProfile.UserName);
        }

        public static string GetUserName(int userId, WishlistContext db)
        {
            var userNameOption = GetUserNameOption(userId, db);

            return userNameOption == null ?
                null :
                userNameOption.Value;
        }
    }
}