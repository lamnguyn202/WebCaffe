using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebCaffe.Models
{
    public class WRatingProduceModel
    {
        public int IsUpdate { get; set; }
        public int Guid { get; set; }

        public int? GuidProduce { get; set; }

        public String FullName { get; set; }

        public String Mobile { get; set; }


        public String Email { get; set; }

        public String Comment { get; set; }

        public String Avatar { get; set; }

        public int? Legit { get; set; }

        [Editable(false)]
        public DateTime? CreatedAt { get; set; }

        public String CreatedBy { get; set; }

        [Editable(false)]
        public DateTime? UpdatedAt { get; set; }

        public String UpdatedBy { get; set; }
    }
}