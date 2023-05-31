using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using WebCaffe.Models;

namespace WebCaffe.Controllers
{
    public class NavController : Controller
    {
        CaffeDataContext db=new CaffeDataContext();
        // GET: Nav
        public ActionResult Index()
        {
            return View();
        }
        public string GetUserInSession()
        {
            try
            {
                //var data = User.Identity;
                //var id = (FormsIdentity)data;
                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null)
                {
                    string encryptedTicket = authCookie.Value;
                    FormsAuthenticationTicket decryptedTicket = FormsAuthentication.Decrypt(encryptedTicket);
                    return decryptedTicket.Name;
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }

        }
        public List<SControlModel> GetAllEditControlInMenu()
        {
            var route = RouteTable.Routes.GetRouteData(HttpContext);
            var linkUrl = route.GetRequiredString("Controller");
            var sMenu = db.SMenus.FirstOrDefault(x => x.LinkUrl != null && x.LinkUrl.Trim() != string.Empty && x.LinkUrl.Trim().ToLower().Contains(("/" + linkUrl).ToLower()));
            Guid? guidMenu = Guid.Empty;
            if (sMenu != null)
            {
                guidMenu = sMenu.Guid;
            }

            var models = new List<SControlModel>();

            string nameUser = "";
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                string encryptedTicket = authCookie.Value;
                FormsAuthenticationTicket decryptedTicket = FormsAuthentication.Decrypt(encryptedTicket);
                nameUser = decryptedTicket.Name;
            }

            var roleControls = db.KhachHangs.Where(s => s.tendangnhap.ToUpper().Equals(nameUser.ToUpper())).Select(x => new { x.marole }).FirstOrDefault();
            if (roleControls != null && roleControls.marole != null)
            {
                models = (from frc in db.FRoleControlMenus
                          join er in db.Roles on frc.GuidRole equals er.marole.ToString()
                          join ec in db.SControls on frc.GuidControl equals ec.Guid
                          join fcm in db.FControlMenus on frc.GuidControl equals fcm.GuidControl
                          where frc.GuidRole == roleControls.marole.ToString() && ec.Type == "EDIT_CONTROL" && (fcm.GuidMenu == guidMenu || guidMenu == Guid.Empty)
                          select new SControlModel()
                          {
                              Guid = ec.Guid,
                              Name = ec.Name,
                              SpriteCssClass = ec.SpriteCssClass,
                              EventClick = ec.EventClick,
                              OrderBy = ec.Orderby,
                              Active = ec.Active,
                          }).OrderBy(s => s.OrderBy).ToList();
            }

            return models;
        }

        public SUserModel GetSettingUser()
        {
            var tmpUsername = GetUserInSession();

            var userInformation = (from ensd in db.KhachHangs
                                   join sr in db.Roles on ensd.marole equals sr.marole into ps
                                   from p in ps.DefaultIfEmpty()
                                   where ensd.tendangnhap.ToUpper().Trim().Equals(tmpUsername.Trim().ToUpper())
                                   select new SUserModel()
                                   {
                                       Guid = ensd.makh.ToString(),
                                       RecordsInPage = 500,
                                       Username = ensd.tendangnhap,
                                       Position = null,
                                       Email = ensd.email,
                                       Mobile = ensd.dienthoai,
                                       IsAdmin = (ensd.Role.tenrole.Equals("ADMIN"))?true:false,
                                       Avartar = "/Content/Custom/user.jpg"
                                   }).FirstOrDefault();
            if (userInformation != null)
            {
                ViewBag.Pagesize = (userInformation.RecordsInPage == null || userInformation.RecordsInPage == 0) ? 10 : userInformation.RecordsInPage;
                return userInformation;
            }
            return new SUserModel();
        }

        public ActionResult Amenu()
        {
            try
            {
                var route = RouteTable.Routes.GetRouteData(HttpContext);
                var currentPortal = "admin";
                if (route != null)
                {
                    currentPortal = route.GetRequiredString("Portal");
                    ViewData["Portal"] = route.GetRequiredString("Portal");
                    var user = GetSettingUser();
                    if (user != null)
                    {
                        ViewData["Username"] = user.Username;
                        ViewData["GuidUser"] = user.Guid.ToString().ToLower().Trim();
                    }
                }

                var models = new List<SMenuModel>();
                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket authTicket = null;
                if (authCookie != null)
                {
                    string encryptedTicket = authCookie.Value;
                    authTicket = FormsAuthentication.Decrypt(encryptedTicket);

                }

                var roleControls = db.KhachHangs.Where(s => s.tendangnhap.ToUpper().Equals(authTicket.Name.ToUpper())).Select(x => new { x.marole }).FirstOrDefault();
                if (roleControls != null && (roleControls.marole) != null)
                {
                    models = (from frc in db.FRoleControlMenus
                              join er in db.Roles on frc.GuidRole.ToString() equals er.marole.ToString()
                              join em in db.SMenus on frc.GuidMenu equals em.Guid
                              where frc.GuidRole.ToString() == roleControls.marole.ToString() && em.Type == "ADMIN"
                              select new SMenuModel()
                              {
                                  Guid = em.Guid.ToString(),
                                  Name = em.Name,
                                  Active = em.Active,
                                  Note = em.Note,
                                  FriendUrl = em.FriendUrl,
                                  GuidLanguage = em.GuidLanguage.ToString(),
                                  GuidParent = em.GuidParent.ToString(),
                                  Icon = string.IsNullOrEmpty(em.Icon) ? "001_Help" : em.Icon,
                                  LinkUrl = em.LinkUrl,
                                  OrderMenu = (int)em.OrderMenu,
                                  RouterUrl = em.RouterUrl,
                                  Type = em.Type,
                              }).OrderBy(s => s.OrderMenu).ToList();
                }
                return View(models);
            }
            catch
            {
                return Redirect("/Error/ErrorList");
            }

        }
        public ActionResult AToolbarItem()
        {
            try
            {
                var models = GetAllEditControlInMenu();

                Session["EditControls"] = models.Select(x => x.EventClick).ToList();

                return View(models);
            }
            catch
            {
                return Redirect("/Error/ErrorList");
            }
        }
        public List<SControlModel> GetAllMainControlInMenu()
        {
            var route = RouteTable.Routes.GetRouteData(HttpContext);
            var linkUrl = route.GetRequiredString("Controller");
            var sMenu = db.SMenus.FirstOrDefault(x => x.LinkUrl != null && x.LinkUrl.Trim() != string.Empty && x.LinkUrl.Trim().ToLower().Contains(("/" + linkUrl).ToLower()));
            Guid? guidMenu = Guid.Empty;
            if (sMenu != null)
            {
                guidMenu = sMenu.Guid;
            }

            var models = new List<SControlModel>();
            var currentUsername = "";
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                string encryptedTicket = authCookie.Value;
                FormsAuthenticationTicket decryptedTicket = FormsAuthentication.Decrypt(encryptedTicket);
                currentUsername = decryptedTicket.Name;
            }
            var roleControls = db.KhachHangs.Where(s => s.tendangnhap.ToUpper().Equals(currentUsername.ToUpper())).Select(x => new { x.marole }).FirstOrDefault();
            if (roleControls != null && roleControls.marole != null)
            {
                models = (from frc in db.FRoleControlMenus
                          join er in db.Roles on frc.GuidRole equals er.marole.ToString()
                          join ec in db.SControls on frc.GuidControl equals ec.Guid
                          join fcm in db.FControlMenus on frc.GuidControl equals fcm.GuidControl
                          where frc.GuidRole == roleControls.marole.ToString() && ec.Type == "MAIN_CONTROL" && (fcm.GuidMenu == guidMenu || guidMenu == Guid.Empty)
                          select new SControlModel()
                          {
                              Guid = ec.Guid,
                              Name = ec.Name,
                              SpriteCssClass = ec.SpriteCssClass,
                              EventClick = ec.EventClick,
                              OrderBy = ec.Orderby,
                              Active = ec.Active,
                          }).OrderBy(s => s.OrderBy).ToList();
            }

            return models;
        }

        public ActionResult AToolbar()
        {
            try
            {
                var models = GetAllMainControlInMenu();

                Session["MainControls"] = models.Select(x => x.EventClick).ToList();

                return View(models);
            }
            catch
            {
                return Redirect("/Error/ErrorList");
            }
        }

    }
}