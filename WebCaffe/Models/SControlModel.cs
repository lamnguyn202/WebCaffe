using Gemini.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebCaffe.Models
{
    public class SControlModel
    {
        public int IsUpdate { get; set; }

        #region Properties
        [ScaffoldColumn(false)]
        public Guid Guid { get; set; }

     
        public String Name { get; set; }

        public bool Active { get; set; }

        public String Note { get; set; }

        public String EventClick { get; set; }

        public String SpriteCssClass { get; set; }

        public String Type { get; set; }

        public int OrderBy { get; set; }

        [Editable(false)]
        public DateTime? CreatedAt { get; set; }

        public String CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public String UpdatedBy { get; set; }
        #endregion

        public bool IsMenu { get; set; }

        public bool IsRole { get; set; }

        #region Constructor
        public SControlModel()
        {
        }

        public SControlModel(SControl sControl)
        {
            Guid = sControl.Guid;
            Name = sControl.Name;
            Active = sControl.Active;
            Note = sControl.Note;
            CreatedAt = sControl.CreatedAt;
            CreatedBy = sControl.CreatedBy;
            UpdatedAt = sControl.UpdatedAt;
            UpdatedBy = sControl.UpdatedBy;
            EventClick = sControl.EventClick;
            SpriteCssClass = sControl.SpriteCssClass;
            Type = sControl.Type;
            OrderBy = sControl.Orderby;
        }
        #endregion

        #region Function
        public void Setvalue(SControl sControl)
        {
            if (IsUpdate == 0)
            {
                sControl.Guid = Guid.NewGuid();
                sControl.CreatedBy = CreatedBy;
                sControl.CreatedAt = DateTime.Now;
            }
            sControl.Name = Name;
            sControl.Active = Active;
            sControl.Note = Note;
            sControl.SpriteCssClass = SpriteCssClass;
            sControl.EventClick = EventClick;
            sControl.UpdatedAt = DateTime.Now;
            sControl.UpdatedBy = UpdatedBy;
            sControl.Type = Type;
            sControl.Orderby = OrderBy;
        }
        #endregion
    }
}