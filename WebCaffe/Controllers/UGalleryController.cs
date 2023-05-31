using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SINNOVA.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebCaffe.Models;
using static WebCaffe.Controllers.SUserController;

namespace WebCaffe.Controllers
{
    public class UGalleryController : Controller
    {
        CaffeDataContext db = new CaffeDataContext();
        // GET: UGallery
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
        public ResponseObj DataReturn =new ResponseObj();
        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            List<UGalleryModel> uGalleryModels = (from ug in db.UGalleries
                                                  select new UGalleryModel
                                                  {
                                                      Guid = ug.Guid,
                                                      Name = ug.Name,
                                                      Description = ug.Description,
                                                      GuidGroup = ug.GuidGroup,
                                                      Link = ug.Link,
                                                      Image = ug.Image,
                                                      Active = (bool)ug.Active,
                                                      Note = ug.Note,
                                                      CreatedAt = ug.CreatedAt,
                                                      CreatedBy = ug.CreatedBy,
                                                      UpdatedAt = ug.UpdatedAt,
                                                      UpdatedBy = ug.UpdatedBy,
                                                  }).OrderBy(s => s.Name).ToList();
            DataSourceResult result = uGalleryModels.ToDataSourceResult(request);
            return Json(result);
        }
        public ActionResult Delete(Guid guid)
        {
            var uGallery = new UGallery();
            try
            {
                uGallery = db.UGalleries.FirstOrDefault(c => c.Guid == guid);

                #region Delete Physical

                var url = uGallery.Image.Replace("%2F", "/");
                if (!string.IsNullOrEmpty(url))
                {
                    var folder = Path.GetDirectoryName(url);
                    var fileNameImage = Path.GetFileName(url);
                    fileNameImage = fileNameImage.Replace("%20", " ");
                    FileInfo fileImage = new FileInfo(Server.MapPath(folder) + "\\" + fileNameImage);
                    if (fileImage.Exists)
                    {
                        fileImage.Delete();
                    }

                    var fullPath = Path.Combine(Server.MapPath(folder));

                    string filesToDelete = @"*" + uGallery.Guid + "*" + ".jpg";
                    try
                    {
                        string[] fileListImage = Directory.GetFiles(fullPath, filesToDelete);
                        foreach (var item in fileListImage)
                        {
                            FileInfo fileImages = new FileInfo(item);
                            if (fileImages.Exists)
                            {
                                fileImages.Delete();
                            }
                        }
                    }
                    catch
                    {

                    }
                }

                #endregion

                #region Delete FProduceGallery

                var fProduceGallery = db.FProduceGalleries.Where(x => x.GuidGallery == guid).ToList();
                if (fProduceGallery.Any())
                {
                    db.FProduceGalleries.DeleteAllOnSubmit(fProduceGallery);
                }

                #endregion

                db.UGalleries.DeleteOnSubmit(uGallery);
                try
                {
                    db.SubmitChanges();
                    DataReturn.ActiveCode = uGallery.Guid.ToString();
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
        public ActionResult ReadProduce([DataSourceRequest] DataSourceRequest request, string guid, string lstFilePath)
        {
            List<UGalleryModel> uGalleryModel = new List<UGalleryModel>();

            if (!string.IsNullOrWhiteSpace(guid))
            {
                uGalleryModel = (from ug in db.UGalleries
                                 join fpg in db.FProduceGalleries on ug.Guid equals fpg.GuidGallery
                                 join pp in db.SanPhams on fpg.GuidProduce equals pp.masp
                                 where pp.masp.ToString() == guid
                                 select new UGalleryModel
                                 {
                                     Guid = ug.Guid,
                                     Name = ug.Name,
                                     Description = ug.Description,
                                     GuidGroup = ug.GuidGroup,
                                     Link = ug.Link,
                                     Image = ug.Image,
                                     Active = (bool)ug.Active,
                                     Note = ug.Note,
                                     IsProduce = true,
                                     CreatedAt = ug.CreatedAt,
                                     CreatedBy = ug.CreatedBy,
                                     UpdatedAt = ug.UpdatedAt,
                                     UpdatedBy = ug.UpdatedBy,
                                 }).OrderByDescending(s => s.CreatedAt).ToList();
            }

            if (!string.IsNullOrWhiteSpace(lstFilePath))
            {
                lstFilePath = lstFilePath.Replace(@"\", @"/");
                uGalleryModel = (from ug in db.UGalleries
                                 select new UGalleryModel
                                 {
                                     Guid = ug.Guid,
                                     Name = ug.Name,
                                     Description = ug.Description,
                                     GuidGroup = ug.GuidGroup,
                                     Link = ug.Link,
                                     Image = ug.Image,
                                     Active = (bool)ug.Active,
                                     Note = ug.Note,
                                     IsProduce = true,
                                     CreatedAt = ug.CreatedAt,
                                     CreatedBy = ug.CreatedBy,
                                     UpdatedAt = ug.UpdatedAt,
                                     UpdatedBy = ug.UpdatedBy,
                                 }).OrderByDescending(s => s.CreatedAt).ToList();
                //where lstFilePath.Contains(ug.Image)
                uGalleryModel = uGalleryModel.Where(t => lstFilePath.Contains(t.Image)).ToList();
            }

            DataSourceResult result = uGalleryModel.ToDataSourceResult(request);
            return Json(result);
        }

    }
}