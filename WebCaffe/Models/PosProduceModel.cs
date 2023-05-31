using Gemini.Resources;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebCaffe.Models
{
    public class PosProduceModel
    {
        public int IsUpdate { get; set; }

        #region Properties
        [ScaffoldColumn(false)]
        public int Guid { get; set; }

        public String Name { get; set; }

        public bool Active { get; set; }

        public String Note { get; set; }

        public String Description { get; set; }

        public Decimal? Price { get; set; }

        public String Unit { get; set; }
        public List<UGalleryModel> ListGallery { get; set; }

        public Guid? GuidPartner { get; set; }

        public int GuidCategory { get; set; }

        public int Amount { get; set; }

        public int? Legit { get; set; }

        public int? LegitCount { get; set; }

        public String GuidTags { get; set; }

        public Decimal? SaleOffPrice { get; set; }

        public String SeoTitle { get; set; }

        public String SeoDescription { get; set; }

        public String SeoFriendUrl { get; set; }

        public int Sort { get; set; }

        public bool TopProduce { get; set; }

        public bool HotProduce { get; set; }

        [ScaffoldColumn(false)]
        public String MGuidTags { get; set; }

        public String Code { get; set; }

        public String Color { get; set; }

        public String Size { get; set; }

        public String Status { get; set; }

        public String FullAddress { get; set; }

        public String Latitude { get; set; }

        public String Longitude { get; set; }

        public int? ApprovedStatus { get; set; }

        public String ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [Editable(false)]
        public DateTime? CreatedAt { get; set; }

        public String CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public String UpdatedBy { get; set; }

        #endregion

        //public List<UGalleryModel> ListGallery { get; set; }

        public List<ImageSize> ListImage { get; set; }

        public String LinkImg0 { get; set; }

        public String LinkImg1 { get; set; }

        public String LinkImg2 { get; set; }

        //public List<SReplaceCode> ReplaceCode { get; set; }

        public String NameCategory { get; set; }

        public String NamePartner { get; set; }

        //public UGalleryModel UGallery { get; set; }

        public String FeaturedImage { get; set; }

        public String ParentSeoFriendUrl { get; set; }

        public bool IsRightRequestApprove { get; set; }

        public bool IsRightApprove { get; set; }

        public String ApprovedStatusName { get; set; }

        public String CategorySeoFriendUrl { get; set; }

        public int? Quantity { get; set; }

        #region Constructor
        public PosProduceModel()
        {
        }

        public PosProduceModel(SanPham posProduce)
        {
            if (posProduce!=null)
            {
                Guid = posProduce.masp;
                Name = posProduce.tensp;
                if(posProduce.Active!=null)
                {
                    Active = (bool)posProduce.Active;
                }
                
                if(posProduce.Note!=null)
                {
                    Note = HttpUtility.HtmlDecode(posProduce.Note);
                }    
                

                UpdatedAt = posProduce.ngaycapnhat;
                if (posProduce.mota != null)
                {
                    Description = HttpUtility.HtmlDecode(posProduce.mota);
                }
                
                Price = posProduce.giaban;
                Unit = "VNĐ";
                GuidPartner = null;
                GuidCategory = (int)posProduce.maloai;
                Amount = (int)posProduce.soluongton;
                GuidTags = null;
                SaleOffPrice = null;
                SeoTitle = null;
                SeoDescription = null;
                SeoFriendUrl = null;
                Sort = 0;
                MGuidTags = null;
                HotProduce = true;
                TopProduce = true;
                MGuidTags = null;
                Code = null;
                Color = null;
                Size = null;
                Status = posProduce.Status;
                FullAddress = null;
                Latitude = null;
                Longitude = null;
                ApprovedStatus = posProduce.ApprovedStatus;
                ApprovedBy = posProduce.ApprovedBy;
                ApprovedAt = posProduce.ApprovedAt;
                Legit = posProduce.Legit;
                LegitCount = posProduce.LegitCount;
            }
        }
        #endregion

        #region Function
        public void Setvalue(SanPham posProduce, string guid = null)
        {
            if (IsUpdate == 0)
            {

                posProduce.CreatedBy = CreatedBy;
                posProduce.CreatedAt = DateTime.Now;
            }
            posProduce.masp = int.Parse(guid);
            posProduce.tensp = Name;
            posProduce.Active = Active;
            posProduce.Note = Note;
            posProduce.ngaycapnhat = DateTime.Now;
            posProduce.mota = Description;
            posProduce.giaban = Price;
            posProduce.soluongton = Amount;
            posProduce.maloai = GuidCategory;
            posProduce.Status = Status;
            posProduce.ApprovedStatus = ApprovedStatus;
            posProduce.ApprovedBy = ApprovedBy;
            posProduce.ApprovedAt = ApprovedAt;
            posProduce.Legit = Legit;
            posProduce.LegitCount = LegitCount;
        }
        #endregion
    }
}