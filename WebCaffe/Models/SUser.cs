using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCaffe.Models
{
    public partial class SUser
    {
        public int Guid { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public bool Active { get; set; }
        public string Note { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public bool Notification { get; set; }
        public int GuidRole { get; set; }
        public string Password { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Skype { get; set; }
        public string Position { get; set; }
        public string Avartar { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> RecordsInPage { get; set; }
        public Nullable<int> Legit { get; set; }
        public Nullable<int> LegitCount { get; set; }
        public string Zalo { get; set; }
        public string Facebook { get; set; }
        public string GuidFollow { get; set; }
        public Nullable<int> OnlineStatus { get; set; }
    }
}