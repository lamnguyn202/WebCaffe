using SINNOVA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using WebCaffe.Models;
using static WebCaffe.Controllers.SUserController;

namespace WebCaffe.Controllers
{
    public class AdminController : Controller
    {
        CaffeDataContext db=new CaffeDataContext();
        // GET: Admin
        public ActionResult Index()
        {
            var route = RouteTable.Routes.GetRouteData(HttpContext);
            var currentPortal = "admin";
            if (route != null)
            {
                currentPortal = route.GetRequiredString("Portal");
            }
            try
            {
                var currentUsername = "";
                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null)
                {
                    string encryptedTicket = authCookie.Value;
                    FormsAuthenticationTicket decryptedTicket = FormsAuthentication.Decrypt(encryptedTicket);
                    currentUsername = decryptedTicket.Name;
                }


                if (string.IsNullOrEmpty(currentUsername))
                {
                    return RedirectToAction("Login", "Admin", new { Portal = currentPortal });
                }


                return View();
            }
            catch (Exception ex)
            {
         
                return RedirectToAction("Login", "Admin", new { Portal = currentPortal });
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            User user = new User();
            user.SecurityCode = (DateTime.Today.Day + DateTime.Today.Month).ToString();
            return View(user);
        }
        [HttpGet]
        public ActionResult Register()
        {
            UserRegister userRegister = new UserRegister();

            return View(userRegister);
        }
        public ResponseObj DataReturn = new ResponseObj();
        public ActionResult Register(UserRegister userRegister, FormCollection registerCollection)
        {
            var _obj = new KhachHang();
            try
            {
                if (!userRegister.Password.Equals(userRegister.PasswordAgain))
                {
                    ViewBag.ErrorAcc = "Mật khẩu không trùng nhau";
                    return View(userRegister);
                }
                else
                {
                    //var response = Request["g-recaptcha-response"];
                    //string secretKey = "6LdMLlkUAAAAAHawi_ylz7A3eUa_ioaGB9DWoq9m";
                    //var client = new WebClient();
                    //var result = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, response));
                    //var obj = JObject.Parse(result);
                    //var status = (bool)obj.SelectToken("success");
                    var status = true;
                    if (status)
                    {
                        if (string.IsNullOrWhiteSpace(userRegister.Email))
                        {
                            ViewBag.ErrorAcc = "Thiếu thông tin Email";
                            return View(userRegister);
                        }

                        var user = db.KhachHangs.FirstOrDefault(s => s.tendangnhap.ToUpper().Equals(userRegister.Username.ToUpper()) || s.tendangnhap.ToUpper().Equals(userRegister.Email.ToUpper()));
                        if (user == null)
                        {
                            var roleUser = db.Roles.FirstOrDefault(s => s.tenrole.ToUpper().Equals("USER"));

                            _obj.makh = db.KhachHangs.Count() + 1;
                            _obj.marole = roleUser.marole;
                            _obj.hoten = userRegister.FullName;
                            _obj.tendangnhap = userRegister.Username;
                            _obj.matkhau = userRegister.Password;
                            _obj.dienthoai = userRegister.Mobile;
                            _obj.email = userRegister.Email;
                            
                            db.KhachHangs.InsertOnSubmit(_obj);
                            try
                            {
                                db.SubmitChanges();
                                return RedirectToAction("Login", "Admin");
                            }
                            catch
                            {
                                return RedirectToAction("Register", "Admin");
                            }

                        }
                        else
                        {
                            ViewBag.ErrorAcc = "Tài khoản đã tồn tại hãy kiểm tra lại email/SĐT và tên đăng nhập!";
                            return View(userRegister);
                        }
                    }
                    else
                    {
                        return View(userRegister);
                    }
                }
            }
            catch (Exception ex)
            {
                KhachHang kh = db.KhachHangs.Where(t => t.makh.ToString().Equals(_obj.makh)).FirstOrDefault();
                if(kh != null)
                {
                    db.KhachHangs.DeleteOnSubmit(kh);
                }

                return Json(DataReturn, JsonRequestBehavior.AllowGet);
            }
        }

        #region Login Post
        [HttpPost]
        public ActionResult Login(User user, FormCollection formCollection)
        {

            
            var route = RouteTable.Routes.GetRouteData(HttpContext);
            var currentPortal = "admin";
            if (route != null)
            {
                currentPortal = route.GetRequiredString("Portal");
                var currentMenu = route.GetRequiredString("Menu");
                ViewData["Portal"] = currentPortal;
                ViewData["Menu"] = currentMenu;
            }
            try
            {
                if (string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.SecurityCode))
                {
                    return RedirectToAction("Login", "Admin", new { Portal = currentPortal });
                }
                if (Convert.ToInt16(user.SecurityCode) == Convert.ToInt16(DateTime.Now.Month + DateTime.Now.Day))
                {
                    var expirationDate = 1;
                    //Duy tri dang  nhap
                    if (user.RememberMe)
                    {
                        expirationDate = 30;
                    }

                    var passWordSha = user.Password.Trim();

                    var erpNsd = db.KhachHangs.FirstOrDefault(s => s.tendangnhap.ToUpper().Equals(user.Username.ToUpper().Trim()) && s.matkhau.ToUpper().Equals(passWordSha.ToUpper()));
                    if (erpNsd != null)
                    {
                            var authTicket = new FormsAuthenticationTicket(1, user.Username, DateTime.Now, DateTime.Now.AddDays(expirationDate), user.RememberMe, vString.GetValueTostring(user.KendoTheme) + ";" + vString.GetValueTostring(user.KendoLanguage));
                            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(authTicket));
                            if (authTicket.IsPersistent)
                            {
                                cookie.Expires = authTicket.Expiration;
                            }
                            cookie.HttpOnly = true;
                            HttpContext.Response.Cookies.Add(cookie);

                            var roleUser = db.Roles.FirstOrDefault(s => s.tenrole.ToUpper().Equals("USER"));
                            if (erpNsd.marole == roleUser.marole)
                                return RedirectToAction("Index", "Home");
                            else
                                return RedirectToAction("Index", "Admin", new { Portal = currentPortal });

                    }

                    ViewBag.ErrorAcc = "Sai tên đăng nhập hoặc mật khẩu. Vui lòng kiểm tra lại.";
                    return View(user);
                }
                ViewBag.ErrorAcc = "Sai mã bảo mật. Vui lòng kiểm tra lại.";
                return View(user);
            }
            catch
            {
                ViewBag.ErrorAcc = "Đăng nhập thất bại. Vui lòng thử lại sau ít phút.";
                return View(user);
            }

        }

        #endregion
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
        #region Logout
        public ActionResult Logout()
        {
            var route = RouteTable.Routes.GetRouteData(HttpContext);
            if (route != null)
            {
                var currentPortal = route.GetRequiredString("Portal");
                var currentMenu = route.GetRequiredString("Menu");
                ViewData["Portal"] = currentPortal;
                ViewData["Menu"] = currentMenu;
            }
            //
            Session.RemoveAll();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Admin");
        }
        #endregion

    }
}