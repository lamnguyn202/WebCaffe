using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace WebCaffe.Models
{
    public class User
    {
        public string SecurityCode { get; set; }

        public string Username { get; set; }

        [DataType(DataType.Password)]

        public string Password { get; set; }

        public string KendoTheme { get; set; }

        public string KendoLanguage { get; set; }

        public bool RememberMe { get; set; }

        public static FormsIdentity Identity { get; set; }
    }
}