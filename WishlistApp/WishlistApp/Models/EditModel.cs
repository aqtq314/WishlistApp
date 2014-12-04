using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WishlistApp.Models
{
    public class RollEditModel
    {
        public string Content { get; set; }
    }

    public class WishlistEditModel
    {
        public class WishlistItemEditModel
        {
            public string Content { get; set; }
        }

        public string Title { get; set; }
        public WishlistItemEditModel[] WishlistItems { get; set; }
    }
}