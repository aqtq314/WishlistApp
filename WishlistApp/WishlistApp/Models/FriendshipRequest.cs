//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WishlistApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class FriendshipRequest
    {
        public int SenderUserId { get; set; }
        public int ReceiverUserId { get; set; }
        public System.DateTime FriendshipRequestTimeUtc { get; set; }
    
        public virtual UserProfile UserProfile { get; set; }
        public virtual UserProfile UserProfile1 { get; set; }
    }
}
