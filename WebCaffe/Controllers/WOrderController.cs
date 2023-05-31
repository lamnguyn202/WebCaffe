using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebCaffe.Models;

namespace WebCaffe.Controllers
{
    public class WOrderController : Controller
    {
        CaffeDataContext db=new CaffeDataContext();
        // GET: WOrder
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
            var username = GetUserInSession();

            List<WOrderModel> wOrders = (from wo in db.DonHangs
                                         join su in db.KhachHangs on wo.makh equals su.makh
                                         where su.tendangnhap == username
                                         select new WOrderModel
                                         {
                                             Guid = wo.madon.ToString(),
                                             OrderCode = wo.madon.ToString(),
                                             Username = su.tendangnhap,
                                             Mobile = wo.dienthoai,
                                             FullAddress = wo.diachi,
                                             CreatedAt = wo.ngaydat
                                         }).ToList();

            DataSourceResult result = wOrders.OrderByDescending(x => x.CreatedAt).ToDataSourceResult(request);
            return Json(result);
        }
        public ActionResult ReadTabc1([DataSourceRequest] DataSourceRequest request, string guid)
        {
            List<WOrderDetailModel> wOrderDetails = (from wod in db.ChiTietDonHangs
                                                     join pp in db.SanPhams on wod.masp equals pp.masp
                                                     where wod.madon.ToString().ToLower() == guid
                                                     select new WOrderDetailModel
                                                     {
                                                         ProduceCode = pp.masp.ToString(),
                                                         ProduceName = pp.tensp,
                                                         Quantity = wod.soluong,
                                                         Price = wod.gia,
                                                         Status = (int)wod.trangthai,
                                                         ListGallery = (from fr in db.FProduceGalleries
                                                                        join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                                        where fr.GuidProduce == pp.masp                                                                      select new UGalleryModel
                                                                        {
                                                                            Image = im.Image,
                                                                            CreatedAt = im.CreatedAt
                                                                        }).OrderBy(x => x.CreatedAt).Take(1).ToList(),
                                                     }).ToList();

            foreach(var item in wOrderDetails)
            {
                item.StatusName = WOrderDetail_Status.dicDesc[item.Status];

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
    }
    public class WOrderDetail_Status
    {
        public const int Inprogress = 0;
        public const int Done = 1;

        public static Dictionary<int, string> dicDesc = new Dictionary<int, string>()
        {
            {Inprogress,                "Đang xử lý"},
            {Done,                      "Hoàn thành"},
        };
    }
}