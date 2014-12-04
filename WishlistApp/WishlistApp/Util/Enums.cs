using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WishlistApp.Util
{
    public enum FriendshipStatus : byte
    {
        NoFriendship = 0,
        FriendshipRequestSent = 1,
        FriendshipRequestPending = 2,
        HasFriendship = 3,
        Self = 4
    }

    public enum Visibility : byte
    {
        Private = 0,
        Public = 1,
        Friend = 2,
    }
}