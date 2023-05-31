using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using System.Xml.Linq;

namespace WebCaffe.Models
{
    public class UserRegister
    {

        public String FullName { get; set; }
        public String Username { get; set; }
        public String Account { get; set; }

        public String Password { get; set; }
        public String PasswordAgain { get; set; }
        public String Mobile { get; set; }

        public String Email { get; set; }

        public String CodeExtend { get; set; }
    }
}