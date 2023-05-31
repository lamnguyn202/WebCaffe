using Antlr.Runtime.Misc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SINNOVA.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using WebCaffe.Models;
using static Kendo.Mvc.UI.UIPrimitives;
using static WebCaffe.Controllers.SUserController;

namespace WebCaffe.Controllers
{
    public class PosProduceController : Controller
    {
        CaffeDataContext db=new CaffeDataContext();
        // GET: PosProduce
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
            var user = GetSettingUser();
            var userControl = Session["MainControls"] as List<string>;

            bool isRightApprove = userControl.Any(x => x.Equals("btnApprove"));
            bool isRightRequestApprove = userControl.Any(x => x.Equals("btnRequestApprove"));

            List<PosProduceModel> posProduceModel = (from pp in db.SanPhams
                                                     join pc in db.Loais on pp.maloai equals pc.maloai
                                                     select new PosProduceModel
                                                     {
                                                         Guid = pp.masp,
                                                         Name = pp.tensp,
                                                         NameCategory = pc.tenloai,
                                                         NamePartner = null,
                                                         Active = true,
                                                         Note = null,
                                                         CreatedAt = pp.ngaycapnhat,
                                                         UpdatedAt = pp.ngaycapnhat,
                                                         Description = pp.mota,
                                                         Price = pp.giaban,
                                                         Unit = "VNĐ",
                                                         GuidCategory = (int)pp.maloai,
                                                         Amount = (int)pp.soluongton,
                                                         ListGallery = (from fr in db.FProduceGalleries
                                                                        join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                                        where fr.GuidProduce == pp.masp
                                                                        select new UGalleryModel
                                                                        {
                                                                            Image = im.Image
                                                                        }).ToList(),
                                                         IsRightApprove = isRightApprove,
                                                         IsRightRequestApprove = isRightRequestApprove,
                                                         ApprovedStatus = pp.ApprovedStatus
                                                     }).OrderBy(s => s.Guid).ToList();

            foreach (var item in posProduceModel)
            {
                var tmpLinkImg = item.ListGallery.ToList();
                if (tmpLinkImg.Count == 0)
                {
                    item.LinkImg0 = "/Content/Custom/empty-album.png";
                    item.LinkImg1 = "/Content/Custom/empty-album.png";
                    item.LinkImg2 = "/Content/Custom/empty-album.png";
                }
                if (tmpLinkImg.Count == 1)
                {
                    item.LinkImg0 = tmpLinkImg[0].Image;
                    item.LinkImg1 = "/Content/Custom/empty-album.png";
                    item.LinkImg2 = "/Content/Custom/empty-album.png";
                }
                else if (tmpLinkImg.Count == 2)
                {
                    item.LinkImg0 = tmpLinkImg[0].Image;
                    item.LinkImg1 = tmpLinkImg[1].Image;
                    item.LinkImg2 = "/Content/Custom/empty-album.png";
                }
                else if (tmpLinkImg.Count > 2)
                {
                    item.LinkImg0 = tmpLinkImg[0].Image;
                    item.LinkImg1 = tmpLinkImg[1].Image;
                    item.LinkImg2 = tmpLinkImg[2].Image;
                }

                if (item.ApprovedStatus != null)
                {
                    item.ApprovedStatusName = PosProduce_ApprovedStatus.dicDesc[item.ApprovedStatus.Value];
                }
            }

            DataSourceResult result = posProduceModel.ToDataSourceResult(request);
            return Json(result);
        }
        public ActionResult Create()
        {
            var user = GetSettingUser();
            ViewBag.IsAdmin = user.IsAdmin;

            var posProduce = new SanPham();
            try
            {
                //var tmp = db.SReplaceCodes.ToList();
                posProduce = new SanPham();
                var listSort = db.SanPhams.Select(s => s.masp).ToList();
                int maxSort = 0;
                if (listSort.Count != 0)
                {
                    maxSort = listSort.Max() + 1;
                }
                int masp = db.SanPhams.Count()+1;
                while(true)
                {
                    SanPham sp =db.SanPhams.Where(t=>t.masp==masp).FirstOrDefault();
                    if(sp==null)
                    { break; }
                    masp++;

                }
                //var viewModel = new PosProduceModel(posProduce) { Guid = masp, IsUpdate = 0, Active = true, Sort = maxSort};
                PosProduceModel viewModel = new PosProduceModel();
                viewModel.Guid = masp;
                viewModel.IsUpdate = 0;
                viewModel.Active=true;
                viewModel.Sort=maxSort;

                return PartialView("Edit", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }

        }
        public ActionResult Edit(string guid)
        {
            var user = GetSettingUser();
            ViewBag.IsAdmin = user.IsAdmin;

            var posProduce = new SanPham();
            try
            {
                //var tmp = db.SReplaceCodes.ToList();
                posProduce = db.SanPhams.FirstOrDefault(c => c.masp.ToString() == guid);
                var viewModel = new PosProduceModel(posProduce) { IsUpdate = 1 };
                return PartialView("Edit", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update(PosProduceModel viewModel)
        {
            var posProduce = new SanPham();
            try
            {
                viewModel.UpdatedBy = viewModel.CreatedBy = GetUserInSession();
                viewModel.Price = viewModel.Price.GetValueOrDefault(0);
                viewModel.Legit = viewModel.Legit.GetValueOrDefault(0);
                viewModel.LegitCount = viewModel.LegitCount.GetValueOrDefault(0);
                if (viewModel.IsUpdate == 0)
                {
                    viewModel.ApprovedStatus = PosProduce_ApprovedStatus.Approving;
                    posProduce.CreatedBy = viewModel.CreatedBy;
                    posProduce.CreatedAt = DateTime.Now;
                    posProduce.masp = viewModel.Guid;
                    posProduce.tensp = viewModel.Name;
                    posProduce.Active = viewModel.Active;
                    posProduce.Note = viewModel.Note;
                    posProduce.ngaycapnhat = DateTime.Now;
                    posProduce.mota = viewModel.Description;
                    posProduce.giaban = viewModel.Price;
                    posProduce.soluongton = viewModel.Amount;
                    posProduce.maloai = viewModel.GuidCategory;
                    posProduce.Status = viewModel.Status;
                    posProduce.ApprovedStatus = viewModel.ApprovedStatus;
                    posProduce.ApprovedBy = viewModel.ApprovedBy;
                    posProduce.ApprovedAt = viewModel.ApprovedAt;
                    posProduce.Legit = viewModel.Legit;
                    posProduce.LegitCount = viewModel.LegitCount;
                    db.SanPhams.InsertOnSubmit(posProduce);
                }
                else if (viewModel.IsUpdate == 1)
                {
                    posProduce = db.SanPhams.FirstOrDefault(c => c.masp == viewModel.Guid);
                    posProduce.tensp = viewModel.Name;
                    posProduce.Active = viewModel.Active;
                    posProduce.Note = viewModel.Note;
                    posProduce.ngaycapnhat = DateTime.Now;
                    posProduce.mota = viewModel.Description;
                    posProduce.giaban = viewModel.Price;
                    posProduce.soluongton = viewModel.Amount;
                    posProduce.maloai = viewModel.GuidCategory;
                    posProduce.Status = viewModel.Status;
                    posProduce.ApprovedStatus = viewModel.ApprovedStatus;
                    posProduce.ApprovedBy = viewModel.ApprovedBy;
                    posProduce.ApprovedAt = viewModel.ApprovedAt;
                    posProduce.Legit = viewModel.Legit;
                    posProduce.LegitCount = viewModel.LegitCount;
                }

                try
                {
                    db.SubmitChanges();
                    DataReturn.ActiveCode = posProduce.masp.ToString();
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                }
                catch
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = "Can not update in db" + " Date : " + DateTime.Now;
                }


            }
            catch (Exception ex)
            {
                if (viewModel.IsUpdate == 0)
                {
                    SanPham sp=db.SanPhams.Where(t=>t.masp==posProduce.masp).FirstOrDefault();
                    if(sp!= null)
                    {
                        db.SanPhams.DeleteOnSubmit(sp);
                        db.SubmitChanges();
                    }
                }
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Delete(string guid)
        {
            var posProduce = new SanPham();
            try
            {
                posProduce = db.SanPhams.FirstOrDefault(c => c.masp.ToString() == guid);

                db.SanPhams.DeleteOnSubmit(posProduce);
                try
                {
                    db.SubmitChanges();
                    DataReturn.ActiveCode = posProduce.masp.ToString();
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                }
                catch
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = "Can not delete in db" + " Date : " + DateTime.Now;
                }
            }
            catch (Exception ex)
            {

            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteFProduceGallery(string guidProduce, string guidGallery)
        {
            try
            {
                var listGuidGallery = guidGallery.Split(';').ToList();
                var listFRemove = db.FProduceGalleries.Where(x => listGuidGallery.Contains(x.GuidGallery.ToString())).ToList();
                db.FProduceGalleries.DeleteAllOnSubmit(listFRemove);

                var listGRemove = db.UGalleries.Where(x => listGuidGallery.Contains(x.Guid.ToString())).ToList();
                db.UGalleries.DeleteAllOnSubmit(listGRemove);

                db.SubmitChanges();

                DataReturn.ActiveCode = guidProduce;
                DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveFProduceGallery(string guidProduce, string guidGallery)
        {
            try
            {
                var listRemove = db.FProduceGalleries.Where(x => x.GuidProduce.ToString() == guidProduce).ToList();
                db.FProduceGalleries.DeleteAllOnSubmit(listRemove);
                var listGuidGallery = guidGallery.Split(';');
                foreach (var item in listGuidGallery)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        FProduceGallery fProduceGallery = new FProduceGallery();
                        fProduceGallery.Guid = Guid.NewGuid();
                        fProduceGallery.CreatedAt = DateTime.Now;
                        fProduceGallery.CreatedBy = GetUserInSession();
                        fProduceGallery.GuidGallery = new Guid(item);
                        fProduceGallery.GuidProduce = int.Parse(guidProduce);
                        db.FProduceGalleries.InsertOnSubmit(fProduceGallery);

                    }
                }
                db.SubmitChanges();

                DataReturn.ActiveCode = guidProduce;
                DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {

            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Save(HttpPostedFileBase File_path1, string guidProduce)
        {
            var physicalPath = "";
            var nameFile = Path.GetFileName(File_path1.FileName);
            guidProduce = guidProduce ?? String.Empty;
            if (File_path1 != null)
            {
                string tmp = Server.MapPath("~/Content/UserFiles/Images/Product/" + guidProduce + "/");
                if (System.IO.File.Exists(Path.Combine(tmp, nameFile)))
                {
                    System.IO.File.Delete(Path.Combine(tmp, nameFile));
                }
                physicalPath = Path.Combine(Server.MapPath("~/Content/UserFiles/Images/Product/" + guidProduce + "/"), nameFile);

                VerifyDir(physicalPath);
                WriteFileFromStream(File_path1.InputStream, physicalPath);
                SaveGallery(nameFile, guidProduce);
            }
            return Json(new { physicalPath = "" + physicalPath + "" }, "text/plain");
        }
        public static void WriteFileFromStream(Stream stream, string toFile)
        {
            using (FileStream fileToSave = new FileStream(toFile, FileMode.Create))
            {
                stream.CopyTo(fileToSave);
            }
        }
        public void SaveGallery(string fileName1, string guidProduce)
        {
            try
            {
                Guid guidGroup = Guid.Empty;

                UGallery uGallery = new UGallery();
                uGallery.Name = fileName1;
                uGallery.Guid = Guid.NewGuid();
                uGallery.GuidGroup = guidGroup;
                uGallery.Active = true;
                uGallery.CreatedAt = uGallery.UpdatedAt = DateTime.Now;
                uGallery.CreatedBy = uGallery.UpdatedBy = GetUserInSession();
                uGallery.Image = "/Content/UserFiles/Images/Product/" + guidProduce + "/" + fileName1;
                db.UGalleries.InsertOnSubmit(uGallery);
                db.SubmitChanges();

                FProduceGallery fProduceGallery = new FProduceGallery();
                fProduceGallery.Guid = Guid.NewGuid();
                fProduceGallery.CreatedAt = DateTime.Now;
                fProduceGallery.CreatedBy = GetUserInSession();
                fProduceGallery.GuidGallery = uGallery.Guid;
                fProduceGallery.GuidProduce = int.Parse(guidProduce);
                db.FProduceGalleries.InsertOnSubmit(fProduceGallery);

                db.SubmitChanges();
            }
            catch (Exception ex)
            {

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
        public ActionResult Approve(string guidProduces)
        {
            try
            {
                var listGuidProduce = guidProduces.Split(';');
                var listPosProduce = db.SanPhams.Where(c => listGuidProduce.Contains(c.masp.ToString())).ToList();

                foreach (var item in listPosProduce)
                {
                    item.ApprovedAt = DateTime.Now;
                    item.ApprovedBy = GetUserInSession();
                    item.ApprovedStatus = PosProduce_ApprovedStatus.Approved;
                }
                try
                {
                    db.SubmitChanges();
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                    if (listPosProduce != null && listPosProduce.Count > 0)
                        DataReturn.ActiveCode = listPosProduce[0].masp.ToString();
                }
                catch
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = "Can not update in db" + " Date : " + DateTime.Now;
                }
            }
            catch (Exception ex)
            {
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Reject(string guidProduces)
        {
            try
            {
                var listGuidProduce = guidProduces.Split(';');
                var listPosProduce = db.SanPhams.Where(c => listGuidProduce.Contains(c.masp.ToString())).ToList();

                foreach (var item in listPosProduce)
                {
                    item.ApprovedAt = DateTime.Now;
                    item.ApprovedBy = GetUserInSession();
                    item.ApprovedStatus = PosProduce_ApprovedStatus.Rejected;
                }

                try
                {
                    db.SubmitChanges();
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                    if (listPosProduce != null && listPosProduce.Count > 0)
                        DataReturn.ActiveCode = listPosProduce[0].masp.ToString();
                }
                catch
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = "Can not update in db" + " Date : " + DateTime.Now;
                }
            }
            catch (Exception ex)
            {

            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

    }
    public class PosProduce_ApprovedStatus
    {
        public const int Approving = 1;
        public const int Approved = 2;
        public const int Rejected = 3;

        public static Dictionary<int, string> dicDesc = new Dictionary<int, string>()
        {
            {Approving, "Đang chờ phê duyệt"},
            {Approved, "Đã phê duyệt"},
            {Rejected, "Đã từ chối"},
        };
    }
    public class MemoryPostedFile : HttpPostedFileBase
    {
        private readonly byte[] fileBytes;

        public MemoryPostedFile(byte[] fileBytes, string fileName)
        {
            this.fileBytes = fileBytes;
            this.FileName = fileName;
            this.InputStream = new MemoryStream(fileBytes);
        }

        public override int ContentLength => fileBytes.Length;

        public override string FileName { get; }

        public override Stream InputStream { get; }
    }
}