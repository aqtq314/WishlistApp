using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WishlistApp.Models
{
    public class UserInfoIDModel
    {
        public int ID { get; set; }
    }

    public class UserInfoJsonModel
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public bool IsSelf { get; set; }
    }

    public class UserInfoCollectionJsonModel
    {
        public UserInfoJsonModel[] Users { get; set; }
    }
}