using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebCaffe.Models
{
    public class WOrderDetailModel
    {
        public int IsUpdate { get; set; }

   
        public Guid Guid { get; set; }

        public string GuidOrder { get; set; }

        public string GuidProduce { get; set; }

        public int? Quantity { get; set; }

        public int Status { get; set; }

        public decimal? Price { get; set; }


        public string StatusName { get; set; }

        public string ProduceLinkImg0 { get; set; }

        public string OrderCode { get; set; }

        public string OrderFullName { get; set; }

        public string OrderFullAddress { get; set; }

        public string OrderMobile { get; set; }

        public string ProduceCode { get; set; }

        public string ProduceName { get; set; }

        public DateTime OrderCreatedAt { get; set; }

        public string OrderCreatedAtString { get; set; }

        public List<UGalleryModel> ListGallery { get; set; }


        public WOrderDetailModel()
        {
        }


    }
}