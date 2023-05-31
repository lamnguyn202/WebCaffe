using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCaffe.Models
{
    public partial class PosCategory
    {
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public bool Active { get; set; }
        public Nullable<System.Guid> ParentGuid { get; set; }
        public Nullable<int> OrderBy { get; set; }
        public string ClassCss { get; set; }
        public bool IsHome { get; set; }
        public Nullable<System.Guid> GuidImage { get; set; }
        public string SeoFriendUrl { get; set; }
        public string SeoTitle { get; set; }
        public string SeoDescription { get; set; }
    }
}