using Gemini.Resources;
using SINNOVA.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace WebCaffe.Models
{
    public class UGalleryModel
    {
        public int IsUpdate { get; set; }

        #region Properties
        [ScaffoldColumn(false)]
        public Guid Guid { get; set; }

        public String Name { get; set; }


        public String Description { get; set; }

        public Guid? GuidGroup { get; set; }

        public String Link { get; set; }


        public String Image { get; set; }

        public bool Active { get; set; }

        public String Note { get; set; }


        public DateTime? CreatedAt { get; set; }

        public String CreatedBy { get; set; }


        public DateTime? UpdatedAt { get; set; }


        public String UpdatedBy { get; set; }
        #endregion

        public bool IsProduce { get; set; }

        public String UrlAnh { get { return vString.FindString(vString.GetValueTostring(Image), "src=\"", "\""); } }

        #region Constructor
        public UGalleryModel()
        {
        }

        public UGalleryModel(UGallery uGallery)
        {
            Guid = uGallery.Guid;
            Name = uGallery.Name;
            Description = HttpUtility.HtmlDecode(uGallery.Description);
            GuidGroup = uGallery.GuidGroup;
            Link = uGallery.Link;
            Image = string.IsNullOrEmpty(uGallery.Image) ? "" : "<img  src=\"" + HttpUtility.UrlDecode(uGallery.Image) + "\"  />";
            Active = (bool)uGallery.Active;
            Note = uGallery.Note;
            CreatedAt = uGallery.CreatedAt;
            CreatedBy = uGallery.CreatedBy;
            UpdatedAt = uGallery.UpdatedAt;
            UpdatedBy = uGallery.UpdatedBy;
        }
        #endregion

        #region Function
        public void Setvalue(UGallery uGallery)
        {
            if (IsUpdate == 0)
            {
                uGallery.Guid = Guid.NewGuid();
                uGallery.CreatedBy = CreatedBy;
                uGallery.CreatedAt = DateTime.Now;
            }
            uGallery.GuidGroup = GuidGroup;
            uGallery.Name = Name;
            uGallery.Description = Description;
            uGallery.Link = Link;
            uGallery.Image = WebUtility.HtmlDecode(UrlAnh).Replace("%2F", "/").Replace("%20", " ");
            uGallery.Active = Active;
            uGallery.Note = Note;
            uGallery.UpdatedAt = DateTime.Now;
            uGallery.UpdatedBy = UpdatedBy;
        }
        #endregion
    }
}