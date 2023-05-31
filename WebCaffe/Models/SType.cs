using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCaffe.Models
{
    public class SType
    {
        public string Guid { get; set; }
        public string KeyType { get; set; }
        public string ValueType { get; set; }
        public bool Active { get; set; }
        public string Note { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public SType(string ValueType, string Note)
        {
            this.ValueType = ValueType;
            this.Note = Note;
        }
    }
}