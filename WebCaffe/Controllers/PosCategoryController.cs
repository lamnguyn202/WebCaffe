using Gemini.Resources;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SINNOVA.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebCaffe.Models;
using static WebCaffe.Controllers.SUserController;

namespace WebCaffe.Controllers
{
    public class PosCategoryController : Controller
    {
        CaffeDataContext db=new CaffeDataContext();
        // GET: PosCategory
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
        private IEnumerable<PosCategoryModel> ConvertIEnumerate(IEnumerable<Loai> source)
        {
            return source.Select(item => new PosCategoryModel(item)).ToList();
        }

        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            List<Loai> posCategory = db.Loais.OrderBy(p => p.maloai).ToList();

            DataSourceResult result = ConvertIEnumerate(posCategory).ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);

        }
 

        public static IList<PosCategoryModel> BuildTree(IEnumerable<PosCategoryModel> source)
        {
            IList<PosCategoryModel> roots = new BindingList<PosCategoryModel>();
            var groups = source.GroupBy(i => i.Guid).ToList();
            var rootgroups = groups.FirstOrDefault();
            if (rootgroups != null)
            {
                roots = rootgroups.ToList();
                if (roots.Count > 0)
                {
                    var dict = groups.ToDictionary(g => g.Key, g => g.ToList());
                    int order = 0;
                    foreach (var t in roots)
                    {
                        order++;
                        t.RootId = order;
                        //AddChildren(t, dict, ref order);
                    }
                }
            }

            return roots;
        }
        public ActionResult Create()
        {
            var posCategory = new Loai();
            try
            {
                var viewModel = new PosCategoryModel(posCategory) { IsUpdate = 0, Active = true };
                return PartialView("Edit", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }

        }
        public ActionResult Edit(string guid)
        {
            var posCategory = new Loai();
            try
            {
                PosCategoryModel viewModel;
                posCategory = db.Loais.FirstOrDefault(c => c.maloai.ToString() == guid);
                if (posCategory == null)
                {
                    viewModel = new PosCategoryModel(posCategory) { IsUpdate = 1 };
                }
                else
                {
                    var parent = db.Loais.FirstOrDefault(x => x.maloai == posCategory.maloai);
                    viewModel = new PosCategoryModel(posCategory) { IsUpdate = 1};
                }
                return PartialView("Edit", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }


        }
        public ResponseObj DataReturn = new ResponseObj();
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update(PosCategoryModel viewModel)
        {
            var posCategory = new Loai();
            try
            {
                var lstErrMsg = ValidateCategory(viewModel);

                if (lstErrMsg.Count > 0)
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = String.Join("<br/>", lstErrMsg);
                }
                else
                {
                
                    if (viewModel.IsUpdate == 0)
                    {
                        Loai loai = new Loai();
                        loai.maloai = db.Loais.Count() + 1;
                        loai.tenloai = viewModel.Name;
                        db.Loais.InsertOnSubmit(loai);
                    }
                    else
                    {
                        posCategory = db.Loais.FirstOrDefault(c => c.maloai.ToString() == viewModel.Guid);
                        posCategory.tenloai=viewModel.Name;
                    }
                    try
                    {
                        db.SubmitChanges();
                        DataReturn.ActiveCode = posCategory.maloai.ToString();
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
                    Loai l =db.Loais.Where(t=>t.maloai.ToString().Equals(viewModel.Guid)).FirstOrDefault();
                    if (l != null)
                    {
                        db.Loais.DeleteOnSubmit(posCategory);
                        db.SubmitChanges();
                    }
                }
            }
            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        private List<string> ValidateCategory(PosCategoryModel viewModel)
        {
            List<string> lstErrMsg = new List<string>();

            var lstUser = db.Loais.Where(c => c.tenloai.Equals(viewModel.Name) && c.maloai.ToString() != viewModel.Guid).ToList();

            if (lstUser.Count > 0)
            {
                lstErrMsg.Add("Tài khoản trùng Tên danh mục!");
            }

            return lstErrMsg;
        }
        public ActionResult Delete(string guid)
        {
            var posCategory = new Loai();
            try
            {
                posCategory = db.Loais.FirstOrDefault(c => c.maloai.ToString() == guid);

                db.Loais.DeleteOnSubmit(posCategory);

                try
                {
                    db.SubmitChanges();
                    DataReturn.ActiveCode = posCategory.maloai.ToString();
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                }
                catch
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = "Can not deelete in db" + " Date : " + DateTime.Now;
                }
            }
            catch (Exception ex)
            {

            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

    }
}