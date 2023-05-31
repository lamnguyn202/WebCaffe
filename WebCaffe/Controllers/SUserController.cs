using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SINNOVA.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using WebCaffe.Models;

namespace WebCaffe.Controllers
{
    public class SUserController : Controller
    {
        CaffeDataContext db = new CaffeDataContext();
        // GET: SUser
        public ActionResult Index()
        {
            return RedirectToAction("List");
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
                                       IsAdmin = (ensd.Role.tenrole.Equals("ADMIN")) ? true : false,
                                       Avartar = "/Content/Custom/user.jpg"
                                   }).FirstOrDefault();
            if (userInformation != null)
            {
                ViewBag.Pagesize = (userInformation.RecordsInPage == null || userInformation.RecordsInPage == 0) ? 10 : userInformation.RecordsInPage;
                return userInformation;
            }
            return new SUserModel();
        }

        public ActionResult List()
        {
            GetSettingUser();
            return View();
        }
        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            List<KhachHang> listKhachHang= db.KhachHangs.OrderBy(p => p.tendangnhap).ToList();
            List<SUser> sUsers = new List<SUser>();


            foreach(var item in listKhachHang)
            {
                SUser sUser = new SUser();
                sUser.Guid=item.makh;
                sUser.GuidRole = (int)item.marole;
                sUser.FullName = item.hoten;
                sUser.Username = item.tendangnhap;
                sUser.Password = item.matkhau;
                sUser.Email = item.email;
                sUser.Mobile = item.dienthoai;
                sUser.Avartar = item.avartar;

                sUsers.Add(sUser);
            }    


            DataSourceResult result = ConvertIEnumerate(sUsers).ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        private IEnumerable<SUserModel> ConvertIEnumerate(IEnumerable<SUser> source)
        {
            return source.Select(item => new SUserModel(item)).ToList();
        }
        private List<string> ValidateUser(SUserModel viewModel)
        {
            List<string> lstErrMsg = new List<string>();

            if (string.IsNullOrWhiteSpace(viewModel.Email))
            {
                lstErrMsg.Add("Thiếu thông tin Email");
            }

            List<KhachHang> lstUser = db.KhachHangs.Where(c =>(c.tendangnhap.Equals(viewModel.Username)
                                                        || c.email.Equals(viewModel.Email))
                                                       && c.makh.ToString() != viewModel.Guid).ToList();

            if (lstUser.Count > 0)
            {
                lstErrMsg.Add("Tài khoản trùng Tên đăng nhập hoặc Email!");
            }

            return lstErrMsg;
        }
        public class ResponseObj
        {
            public int StatusCode { get; set; }
            public String MessagError { get; set; }
            public int ActiveId { get; set; }
            public String ActiveCode { get; set; }
            public int ActiveRootId { get; set; }
            public String MessagSuccess { get; set; }
        }
        public ResponseObj DataReturn =new ResponseObj();
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
        public static string Key = "CoreworkByDuong2022";
        public static string Encrypt(string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(Key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public ActionResult EditPopup(string guid)
        {
            try
            {
                var sUsers = new KhachHang();
                sUsers = db.KhachHangs.FirstOrDefault(c => c.makh.ToString() == guid);
                var viewModel = new SUserModel();
                if(sUsers != null)
                {
                    viewModel.IsUpdate = 1;
                    viewModel.Guid = guid;
                    viewModel.Username = sUsers.tendangnhap;
                    viewModel.FullName = sUsers.hoten;
                    viewModel.Active = true;
                    viewModel.Email = sUsers.email;
                    viewModel.Avartar = sUsers.avartar;
                    viewModel.Mobile = sUsers.dienthoai;
                    viewModel.Password = sUsers.matkhau;
                    viewModel.Mobile = sUsers.dienthoai;
                }
                return PartialView("EditPopup", viewModel);
            }
            catch
            {
                return Redirect("/Error/ErrorList");
            }
        }
        public ActionResult Save(HttpPostedFileBase File_path1, string guidUser)
        {
            var msg = "";
            var nameFile = Path.GetFileName(File_path1.FileName);
            guidUser = guidUser ?? String.Empty;
            if (File_path1 != null)
            {
                string tmp = Server.MapPath("~/Content/UserFiles/Images/User/" + guidUser + "/");
                if (System.IO.File.Exists(Path.Combine(tmp, nameFile)))
                {
                    System.IO.File.Delete(Path.Combine(tmp, nameFile));
                }
                var physicalPath = Path.Combine(Server.MapPath("~/Content/UserFiles/Images/User/" + guidUser + "/"), nameFile);

                VerifyDir(physicalPath);
                WriteFileFromStream(File_path1.InputStream, physicalPath);
                msg = nameFile;

                var user = db.KhachHangs.FirstOrDefault(x => x.makh.ToString().ToLower().Trim() == guidUser.ToLower().Trim());
                if (user != null)
                {
                    user.avartar = "/Content/UserFiles/Images/User/" + guidUser + "/" + nameFile;
                    db.SubmitChanges();
                }
            }
            return Json(new { status = "" + msg + "" }, "text/plain");
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update(SUserModel viewModel)
        {
            var sUsers = new KhachHang();
            try
            {
                var lstErrMsg = ValidateUser(viewModel);

                if (lstErrMsg.Count > 0)
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = String.Join("<br/>", lstErrMsg);
                }
                else
                {
                    viewModel.UpdatedBy = viewModel.CreatedBy = GetUserInSession();
                    viewModel.Legit = viewModel.Legit.GetValueOrDefault(0);
                    viewModel.LegitCount = viewModel.LegitCount.GetValueOrDefault(0);
                    if (viewModel.IsUpdate == 0)
                    {
                        viewModel.Password = viewModel.Password;

                        sUsers.makh = db.KhachHangs.Count() + 1;
                        sUsers.hoten=viewModel.FullName;
                        sUsers.tendangnhap=viewModel.Username;
                        sUsers.email=viewModel.Email;
                        sUsers.avartar = viewModel.Avartar;
                        sUsers.dienthoai = viewModel.Mobile;
                        sUsers.matkhau = viewModel.Password;
                        if(viewModel.GuidRole.ToString().Length<3)
                            sUsers.marole= viewModel.GuidRole;
                        else
                            sUsers.marole = 1;

                        db.KhachHangs.InsertOnSubmit(sUsers);
                      
                    }
                    else
                    {
                        sUsers = db.KhachHangs.FirstOrDefault(c => c.makh.ToString() == viewModel.Guid);
                        viewModel.Password = string.IsNullOrEmpty(viewModel.Password) ? sUsers.matkhau : Encrypt(viewModel.Password);

                        sUsers.makh = int.Parse(viewModel.Guid);
                        sUsers.hoten = viewModel.FullName;
                        sUsers.tendangnhap = viewModel.Username;
                        sUsers.email = viewModel.Email;
                        sUsers.avartar = viewModel.Avartar;
                        sUsers.dienthoai = viewModel.Mobile;
                        sUsers.matkhau = viewModel.Password;
                 
                    }
                    try
                    {
                        db.SubmitChanges();
                        DataReturn.ActiveCode = sUsers.makh.ToString();
                        DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                    }
                    catch
                    {
                        DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                        DataReturn.MessagError = "Can not update in db" + " Date : " + DateTime.Now;

                    }
                }
            }
            catch (Exception ex)
            {
                if (viewModel.IsUpdate == 0)
                {
                    KhachHang kh=db.KhachHangs.FirstOrDefault(h => h.makh == sUsers.makh);
                    if(kh!=null)
                        db.KhachHangs.DeleteOnSubmit(sUsers);
                }
            }
            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }
        public bool SaveData(string nameTable)
        {
            try
            {
                db.SubmitChanges();
                return true;
            }
            catch
            {
                return false;   
            }
        }

        public static void VerifyDir(string path)
        {
            try
            {
                var list = path.Split(new string[] { "\\" }, StringSplitOptions.None);
                var directory = path.Replace("\\" + list[list.Count() - 1], "");
                DirectoryInfo dir = new DirectoryInfo(directory);
                if (!dir.Exists)
                {
                    dir.Create();
                }
            }
            catch { }
        }
        public ActionResult Delete(string guid)
        {
            try
            {
                var sUsers = new KhachHang();
                sUsers = db.KhachHangs.FirstOrDefault(c => c.makh.ToString() == guid);
                
                

                try
                {
                    if (sUsers != null)
                    {
                        db.KhachHangs.DeleteOnSubmit(sUsers);
                        db.SubmitChanges();
                        DataReturn.ActiveCode = sUsers.makh.ToString();
                        DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                    } 
                }
                catch
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.BadRequest);
                    DataReturn.MessagError = "Can not delete in db" + " Date : " + DateTime.Now;
                }


            }
            catch (Exception ex)
            {
             
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }
        public static void WriteFileFromStream(Stream stream, string toFile)
        {
            using (FileStream fileToSave = new FileStream(toFile, FileMode.Create))
            {
                stream.CopyTo(fileToSave);
            }
        }
        public ActionResult Create()
        {
            try
            {
                var sUsers = new SUser();
                var viewModel = new SUserModel(sUsers) { IsUpdate = 0, Active = true };
                return PartialView("Edit", viewModel);
            }
            catch
            {
                return Redirect("/Error/ErrorList");
            }
        }
        public ActionResult Edit(int guid)
        {
            try
            {
                var sUsers = new SUser();
                KhachHang kh = db.KhachHangs.FirstOrDefault(c => c.makh == guid);

                if (kh == null)
                {
                    sUsers.Guid = kh.makh;
                    sUsers.GuidRole = (int)kh.marole;
                    sUsers.FullName = kh.hoten;
                    sUsers.Username = kh.tendangnhap;
                    sUsers.Password = kh.matkhau;
                    sUsers.Email = kh.email;
                    sUsers.Mobile = kh.dienthoai;
                    sUsers.Avartar = kh.avartar;
                }
                var viewModel = new SUserModel(sUsers) { IsUpdate = 1 };
                return PartialView("Edit", viewModel);
            }
            catch
            {
                return Redirect("/Error/ErrorList");
            }
        }
    }
}