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
    
    public partial class WishlistItem
    {
        public int WishlistItemId { get; set; }
        public int WishlistId { get; set; }
        public string Content { get; set; }
    
        public virtual Wishlist Wishlist { get; set; }
    }
}
