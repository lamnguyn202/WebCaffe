using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SINNOVA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebCaffe.Models;
using static WebCaffe.Controllers.SUserController;

namespace WebCaffe.Controllers
{
    public class UNewsController : Controller
    {
        CaffeDataContext db = new CaffeDataContext();
        // GET: UNews
        public ActionResult Index()
        {
            return RedirectToAction("List");
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
            var user = GetSettingUser();

            List<UNewsModel> uNewsModel = (from un in db.UNews
                                           where un.CreatedBy == user.Username
                                                 || user.IsAdmin
                                           select new UNewsModel
                                           {
                                               Name = un.Name,
                                               Guid = un.Guid,
                                               SeoFriendUrl = un.SeoFriendUrl,
                                               SeoTitle = un.SeoTitle,
                                               SeoDescription = un.SeoDescription,
                                               SeoImage = un.SeoImage,
                                               ContentNews = un.ContentNews,
                                               TypeNews = un.TypeNews,
                                               GuidImage = un.GuidImage,
                                               Description = un.Description,
                                               CountView = un.CountView,
                                               Active = (bool)un.Active,
                                               Note = un.Note,
                                               Status = (int)un.Status,
                                               CreatedAt = un.CreatedAt,
                                               CreatedBy = un.CreatedBy,
                                               UpdatedAt = un.UpdatedAt,
                                               UpdatedBy = un.UpdatedBy
                                           }).OrderBy(s => s.Name).ToList();

            uNewsModel.ForEach(x => x.StatusName = UNews_Status.dicDesc[x.Status]);

            DataSourceResult result = uNewsModel.ToDataSourceResult(request);
            return Json(result);
        }
        public ActionResult Create()
        {
            var uNew = new UNew();
            try
            {

                var viewModel = new UNewsModel(uNew) { IsUpdate = 0, Active = true };
                return PartialView("Edit", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }
        }
        public ActionResult Edit(Guid guid)
        {
            var uNew = new UNew();
            try
            {

                uNew = db.UNews.FirstOrDefault(c => c.Guid == guid);
                var viewModel = new UNewsModel(uNew) { IsUpdate = 1 };
                if (uNew != null) viewModel.ContentNews = HttpUtility.HtmlDecode(uNew.ContentNews);
                return PartialView("Edit", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }
        }
        public ResponseObj DataReturn = new ResponseObj();
        public ActionResult Delete(Guid guid)
        {
            var uNew = new UNew();
            try
            {
                uNew = db.UNews.FirstOrDefault(c => c.Guid == guid);
                db.UNews.DeleteOnSubmit(uNew);
                try
                {
                    db.SubmitChanges();
                    DataReturn.ActiveCode = uNew.Guid.ToString();
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
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
        private List<string> Validate_Update(UNewsModel uNew)
        {
            List<string> lstErrMsg = new List<string>();

            if (uNew.Status >= UNews_Status.Approved)
            {
                lstErrMsg.Add("Tin đã qua bước kiểm định, không thể sửa!");
            }

            return lstErrMsg;
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update(UNewsModel viewModel)
        {
            var uNew = new UNew();
            try
            {
                var lstErrMsg = Validate_Update(viewModel);

                if (lstErrMsg.Count > 0)
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = String.Join("<br/>", lstErrMsg);
                }
                else
                {
                    viewModel.UpdatedBy = viewModel.CreatedBy = GetUserInSession();
                    if (viewModel.IsUpdate == 0)
                    {
                        viewModel.Setvalue(uNew);
                        db.UNews.InsertOnSubmit(uNew);
                    }
                    else if (viewModel.IsUpdate == 1)
                    {
                        uNew = db.UNews.FirstOrDefault(c => c.Guid == viewModel.Guid);
                        viewModel.Setvalue(uNew);
                    }
                    uNew.SeoFriendUrl = uNew.Guid.ToString();

                    try
                    {
                        db.SubmitChanges();
                        DataReturn.ActiveCode = uNew.Guid.ToString();
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
                    UNew n = db.UNews.Where(t => t.Guid == viewModel.Guid).FirstOrDefault();
                    if (n != null)
                    {
                        db.UNews.DeleteOnSubmit(uNew);
                        db.SubmitChanges();
                    }

                }
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }
        private List<string> Validate_Approval(UNew uNew)
        {
            List<string> lstErrMsg = new List<string>();

            if (uNew.Status >= UNews_Status.Approved)
            {
                lstErrMsg.Add("Tin đã qua bước kiểm định, không thể sửa!");
            }

            return lstErrMsg;
        }
        public ActionResult Approve(Guid guid)
        {
            var uNew = new UNew();
            try
            {
                uNew = db.UNews.FirstOrDefault(c => c.Guid == guid);
                var lstErrMsg = Validate_Approval(uNew);

                if (lstErrMsg.Count > 0)
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = String.Join("<br/>", lstErrMsg);
                }
                else
                {
                    uNew.UpdatedAt = DateTime.Now;
                    uNew.UpdatedBy = GetUserInSession();
                    uNew.Status = UNews_Status.Approved;
                    try
                    {
                        db.SubmitChanges();
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

            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Reject(Guid guid)
        {
            var uNew = new UNew();
            try
            {
                uNew = db.UNews.FirstOrDefault(c => c.Guid == guid);
                var lstErrMsg = Validate_Approval(uNew);

                if (lstErrMsg.Count > 0)
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = String.Join("<br/>", lstErrMsg);
                }
                else
                {
                    uNew.UpdatedAt = DateTime.Now;
                    uNew.UpdatedBy = GetUserInSession();
                    uNew.Status = UNews_Status.Reject;

                    try
                    {
                        db.SubmitChanges();
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
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }


    }
    public class UNews_Status
    {
        public const int Approving = 0;
        public const int Approved = 1;
        public const int Reject = 2;

        public static Dictionary<int, string> dicDesc = new Dictionary<int, string>()
        {
            {Approving,                 "Chưa kiểm duyệt"},
            {Approved,                  "Đã kiểm duyệt"},
            {Reject,                    "Từ chối kiểm duyệt"},
        };
    }
}