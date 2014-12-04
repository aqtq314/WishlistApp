using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WishlistApp.Models;
using WishlistApp.Lib;
using WishlistApp.Util;
using WishlistApp.Filters;

namespace WishlistApp.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class FriendshipController : Controller
    {
        public ViewResult Search()
        {
            return View();
        }

        public JsonResult SearchUser(string userName)
        {
            using (var db = new WishlistContext())
            {
                var users =
                    db.UserProfiles
                    .Where(u => u.UserName.Contains(userName))
                    .Join(
                        db.webpages_Membership,
                        u => u.UserId,
                        m => m.UserId,
                        (u, m) => new { u.UserId, u.UserName, m.CreateDate })
                    .AsEnumerable()
                    .Select(u => new
                    {
                        u.UserId,
                        u.UserName,
                        CreationDate = u.CreateDate.ToString(),
                    })
                    .ToList();

                return Json(new
                {
                    Success = true,
                    Users = users
                });
            }
        }

        public JsonResult GetFriends(int userId)
        {
            using (var db = new WishlistContext())
            {
                var friends =
                    Queryable.Union(
                        db.Friendships.Where(fs => fs.UserIdLower == userId)
                            .Select(fs => new { UserId = fs.UserIdUpper, fs.FriendshipSinceUtc }),
                        db.Friendships.Where(fs => fs.UserIdUpper == userId)
                            .Select(fs => new { UserId = fs.UserIdLower, fs.FriendshipSinceUtc }))
                    .Join(
                        db.UserProfiles,
                        fs => fs.UserId,
                        u => u.UserId,
                        (fs, u) => new { fs.UserId, u.UserName, fs.FriendshipSinceUtc })
                    .AsEnumerable()
                    .ToList();

                return Json(friends);
            }
        }

        public JsonResult GetFriendshipStatus(int userId)
        {
            return Context.New((db, currUserId) =>
            {
                var friendshipStatus = ((Func<FriendshipStatus>)(() =>
                {
                    if (userId == currUserId)
                        return FriendshipStatus.Self;

                    var userIdLower = Math.Min(userId, currUserId);
                    var userIdUpper = Math.Max(userId, currUserId);

                    var hasFriendship =
                        db.Friendships.Any(fs =>
                            fs.UserIdLower == userIdLower && fs.UserIdUpper == userIdUpper);
                    if (hasFriendship)
                        return FriendshipStatus.HasFriendship;

                    var hasPendingRequest =
                        db.FriendshipRequests.Any(fsr =>
                            fsr.SenderUserId == userId && fsr.ReceiverUserId == currUserId);
                    if (hasPendingRequest)
                        return FriendshipStatus.FriendshipRequestPending;

                    var hasSentRequest =
                        db.FriendshipRequests.Any(fsr =>
                            fsr.SenderUserId == currUserId && fsr.ReceiverUserId == userId);
                    if (hasSentRequest)
                        return FriendshipStatus.FriendshipRequestSent;

                    return FriendshipStatus.NoFriendship;
                }))();

                return Json(new
                {
                    Success = true,
                    Status = (byte)friendshipStatus
                });
            });
        }

        public JsonResult Befriend(int userId, bool befriend)
        {
            return Context.New((db, currUserId) =>
            {
                var friendshipStatus = ((Func<FriendshipStatus>)(() =>
                {
                    if (userId == currUserId)
                        return FriendshipStatus.Self;

                    var userIdLower = Math.Min(userId, currUserId);
                    var userIdUpper = Math.Max(userId, currUserId);

                    var friendship =
                        db.Friendships.FirstOrDefault(fs =>
                            fs.UserIdLower == userIdLower && fs.UserIdUpper == userIdUpper);
                    if (friendship != null)
                    {
                        if (befriend)
                        {
                            return FriendshipStatus.HasFriendship;
                        }
                        else
                        {
                            db.Friendships.Remove(friendship);
                            db.SaveChanges();

                            return FriendshipStatus.NoFriendship;
                        }
                    }

                    var pendingRequest =
                        db.FriendshipRequests.FirstOrDefault(fsr =>
                            fsr.SenderUserId == userId && fsr.ReceiverUserId == currUserId);
                    if (pendingRequest != null)
                    {
                        if (befriend)
                        {
                            db.FriendshipRequests.Remove(pendingRequest);
                            db.Friendships.Add(new Friendship
                            {
                                UserIdLower = userIdLower,
                                UserIdUpper = userIdUpper
                            });

                            db.SaveChanges();

                            return FriendshipStatus.HasFriendship;
                        }
                        else
                        {
                            db.FriendshipRequests.Remove(pendingRequest);
                            db.SaveChanges();

                            return FriendshipStatus.NoFriendship;
                        }
                    }

                    var sentRequest =
                        db.FriendshipRequests.FirstOrDefault(fsr =>
                            fsr.SenderUserId == currUserId && fsr.ReceiverUserId == userId);
                    if (sentRequest != null)
                    {
                        if (befriend)
                        {
                            return FriendshipStatus.FriendshipRequestSent;
                        }
                        else
                        {
                            db.FriendshipRequests.Remove(sentRequest);
                            db.SaveChanges();

                            return FriendshipStatus.NoFriendship;
                        }
                    }

                    if (befriend)
                    {
                        db.FriendshipRequests.Add(new FriendshipRequest
                        {
                            SenderUserId = currUserId,
                            ReceiverUserId = userId
                        });

                        db.SaveChanges();

                        return FriendshipStatus.FriendshipRequestSent;
                    }

                    return FriendshipStatus.NoFriendship;
                }))();

                return Json(new
                {
                    Success = true,
                    Status = (byte)friendshipStatus
                });
            });
        }
    }
}
