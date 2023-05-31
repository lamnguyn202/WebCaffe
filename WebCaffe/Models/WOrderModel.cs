using Gemini.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebCaffe.Models
{
    public class WOrderModel
    {
        public int IsUpdate { get; set; }

        #region Properties
        [ScaffoldColumn(false)]
        public string Guid { get; set; }

        public string GuidUser { get; set; }

        public String FullAddress { get; set; }

        public String Mobile { get; set; }

        [Editable(false)]
        public DateTime? CreatedAt { get; set; }

        [Editable(false)]
        public String CreatedBy { get; set; }

        [Editable(false)]
        public DateTime? UpdatedAt { get; set; }

        [Editable(false)]
        public String UpdatedBy { get; set; }

        #endregion

        public string OrderCode { get; set; }

        public string Username { get; set; }

        public string StatusName { get; set; }
        

        public WOrderModel()
        {
        }


    }
}