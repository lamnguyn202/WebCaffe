
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebCaffe.Models
{
    public class SUserModel
    {
        public int IsUpdate { get; set; }

        #region Properties

        public string Guid { get; set; }

        public String Username { get; set; }

        public String FullName { get; set; }

        public bool Active { get; set; }

        public String Note { get; set; }

        public String Email { get; set; }

        public String Avartar { get; set; }

        public String Mobile { get; set; }

        public String Password { get; set; }

        public String Position { get; set; }

        public String Skype { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartDate { get; set; }

        public int GuidRole { get; set; }

        public Boolean Notification { get; set; }

        public int? RecordsInPage { get; set; }

        public int? Legit { get; set; }

        public int? LegitCount { get; set; }

        public string GuidFollow { get; set; }

        public String Zalo { get; set; }

        public String Facebook { get; set; }

        public DateTime? CreatedAt { get; set; }

        public String CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public String UpdatedBy { get; set; }
        #endregion

        public bool IsAdmin { get; set; }

        public bool IsChangePassword { get; set; }

        public String Password1 { get; set; }

        public int CountFollow { get; set; }
        public SUserModel()
        {

        }
        public SUserModel(SUser sUser)
        {
            if (sUser != null)
            {
                Guid = sUser.Guid.ToString();
                FullName = sUser.FullName;
                Username = sUser.Username;
                Active = sUser.Active;
                Note = sUser.Note;
                CreatedAt = sUser.CreatedAt;
                CreatedBy = sUser.CreatedBy;
                UpdatedAt = sUser.UpdatedAt;
                UpdatedBy = sUser.UpdatedBy;
                Avartar = sUser.Avartar;
                Email = sUser.Email;
                RecordsInPage = sUser.RecordsInPage;
                EndDate = sUser.EndDate;
                GuidRole = sUser.GuidRole;
                Mobile = sUser.Mobile;
                Notification = sUser.Notification;
                Password = sUser.Password;
                Position = sUser.Position;
                Skype = sUser.Skype;
                StartDate = sUser.StartDate;
                Legit = sUser.Legit;
                LegitCount = sUser.LegitCount;
                Zalo = sUser.Zalo;
                Facebook = sUser.Facebook;
                GuidFollow = sUser.GuidFollow;
            }
        }
    }
}