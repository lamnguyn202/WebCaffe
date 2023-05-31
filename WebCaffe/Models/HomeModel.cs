using Antlr.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCaffe.Models
{
    public class HomeModel
    {
        
        public List<PosCategoryModel> ListPosCategory { get; set; }
    }
    public class ProduceListBySearchModel
    {
        public string KeyWord { get; set; }

        public List<PosCategoryModel> ListPosCategory { get; set; }

        public List<PosProduceModel> ListPosProduceLatest { get; set; }

        public List<PosProduceModel> ListPosProduceBySearch { get; set; }
    }
    public class NewsDetailModel
    {
        public List<PosCategoryModel> ListPosCategory { get; set; }

        public UNewsModel UNews { get; set; }

        public List<UNewsModel> ListNewsSameCreatedBy { get; set; }
    }
    public class NewsListModel
    {
        public List<PosCategoryModel> ListPosCategory { get; set; }

        public List<UNewsModel> ListNews { get; set; }
    }
    public class HeaderModel
    {
        public string CurrentUsername { get; set; }

        public List<SMenuModel> ListMenu { get; set; }
    }
    public class FooterModel
    {
        public List<PosCategoryModel> ListPosCategory { get; set; }
    }
    public class ContactUsModel
    {
        public List<PosCategoryModel> ListPosCategory { get; set; }
    }
    public class PosCategoryModel
    {
        public int IsUpdate { get; set; }
        public bool Active { get; set; }

        public string SeoFriendUrl { get; set; }
        public string Name { get; set; }
        public string Guid { get; set; }
        public int RootId { get; set; }
        public PosCategoryModel(Loai posCategory)
        {
            Guid = posCategory.maloai.ToString();
            Name = posCategory.tenloai;
        }
        public PosCategoryModel() { }
    }
    public class SMenuModel
    {
        public string Guid { get; set; }
        public string LinkUrl { get; set; }
        public bool Active { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string FriendUrl { get; set; }
        public string GuidLanguage { get; set; }
        public string GuidParent { get; set; }
        public string Icon { get; set; }
        public string RouterUrl { get; set; }
        public string Type { get; set; }
        public int OrderMenu { get; set; }
        public SMenuModel(string LinkUrl, string Name, int OrderMenu)
        {
            this.LinkUrl = LinkUrl;
            this.Name = Name;
            this.OrderMenu = OrderMenu;
        }
        public SMenuModel()
        {
            this.Guid = Guid;
            this.Name = Name;
            this.Active = Active;
            this.Note= Note;
            this.FriendUrl = FriendUrl;
            this.GuidLanguage = GuidLanguage;
            this.GuidParent = GuidParent;
            this.Icon = Icon;
            this.LinkUrl= LinkUrl;
            this.OrderMenu= OrderMenu;
            this.RouterUrl= RouterUrl;
            this.Type = Type;
        }
    }
    public class PartialLatestProductModel
    {
        public List<PosProduceModel> ListPosProduce { get; set; }

        public List<PosCategoryModel> ListPosCategory { get; set; }
    }
    public class PartialFeaturedProductModel
    {
        public List<PosProduceModel> ListPosProduce { get; set; }
    }
    public class ProduceListByCategoryModel
    {
        public PosCategoryModel PosCategory { get; set; }

        public List<PosCategoryModel> ListPosCategory { get; set; }

        public List<PosProduceModel> ListPosProduceLatest { get; set; }

        public List<PosProduceModel> ListPosProduceByCategory { get; set; }
    }
    public class ProduceDetailModel
    {
        public SUserModel SUser { get; set; }

        public List<PosCategoryModel> ListPosCategory { get; set; }

        public PosProduceModel PosProduce { get; set; }

        public SUser PosProduceCreatedBy { get; set; }

        public WRatingProduceModel NewRatingProduce { get; set; }

        public List<PosProduceModel> ListProduceSameCreatedBy { get; set; }

        public List<PosProduceModel> ListProduceSameCategory { get; set; }
    }
    public class ProduceCartModel
    {
        public List<PosCategoryModel> ListPosCategory { get; set; }

        public List<PosProduceModel> ListPosProduceCart { get; set; }

        public Dictionary<string, decimal> ListTotalByUnit { get; set; }
    }
    public class ProduceCartCookieModel
    {
        public string GuidProduce { get; set; }
        public int Quantity { get; set; }
    }
}