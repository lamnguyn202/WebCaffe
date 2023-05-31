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
    public class WSellController : Controller
    {
        CaffeDataContext db = new CaffeDataContext();
        // GET: WSell
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// List view grid
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            GetSettingUser();
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
        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            var username = GetUserInSession();

            List<WOrderDetailModel> wOrderDetails = (from wod in db.ChiTietDonHangs
                                                     join pp in db.SanPhams on wod.masp equals pp.masp
                                                     join wo in db.DonHangs on wod.madon equals wo.madon
                                                     join su in db.KhachHangs on wo.makh equals su.makh
                                                     select new WOrderDetailModel
                                                     {
                                                         OrderFullAddress = wo.diachi,
                                                         OrderFullName = su.hoten,
                                                         OrderMobile = wo.dienthoai,
                                                         OrderCreatedAt = (DateTime)wo.ngaydat,

                                                         ProduceCode = pp.masp.ToString(),
                                                         ProduceName = pp.tensp,
                                                 
                                                         GuidOrder = wod.madon.ToString(),
                                                         GuidProduce= pp.masp.ToString(),   
                                                         Quantity = wod.soluong,
                                                         Price = wod.gia,
                                                         Status = (int)wod.trangthai,
                                                         ListGallery = (from fr in db.FProduceGalleries
                                                                        join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                                        where fr.GuidProduce == pp.masp
                                                                        select new UGalleryModel
                                                                        {
                                                                            Image = im.Image,
                                                                            CreatedAt = im.CreatedAt
                                                                        }).OrderBy(x => x.CreatedAt).Take(1).ToList(),
                                                     }).OrderByDescending(x => x.OrderCreatedAt).ToList();

            foreach (var item in wOrderDetails)
            {
                item.StatusName = WOrderDetail_Status.dicDesc[item.Status];
                item.OrderCode = item.GuidOrder.ToString();

                var tmpLinkImg = item.ListGallery;
                if (tmpLinkImg.Count == 0)
                {
                    item.ProduceLinkImg0 = "/Content/Custom/empty-album.png";
                }
                else
                {
                    item.ProduceLinkImg0 = tmpLinkImg[0].Image;
                }
            }

            DataSourceResult result = wOrderDetails.ToDataSourceResult(request);
            return Json(result);
        }


        public ResponseObj DataReturn =new ResponseObj();
        public ActionResult Approve(string GuidOrder, string GuidProduce)
        {
            var wOrderDetail = new ChiTietDonHang();
            try
            {
                wOrderDetail = db.ChiTietDonHangs.FirstOrDefault(c => c.madon.ToString().Equals(GuidOrder) && c.masp.Equals(GuidProduce));
                var lstErrMsg = Validate_Approval(wOrderDetail);

                if (lstErrMsg.Count > 0)
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = String.Join("<br/>", lstErrMsg);
                }
                else
                {
                    wOrderDetail.trangthai = WOrderDetail_Status.Done;

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
        private List<string> Validate_Approval(ChiTietDonHang wOrderDetail)
        {
            List<string> lstErrMsg = new List<string>();

            if (wOrderDetail.trangthai >= WOrderDetail_Status.Done)
            {
                lstErrMsg.Add("Đơn hàng đã hoàn thành, không thể sửa!");
            }

            return lstErrMsg;
        }

    }
}