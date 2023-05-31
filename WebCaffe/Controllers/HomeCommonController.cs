using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebCaffe.Models;
using log4net;
using SINNOVA.Core;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebCaffe.Controllers
{
    public class HomeCommonController : Controller
    {
        CaffeDataContext db=new CaffeDataContext();
        // GET: HomeCommon
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
                if(authCookie!= null)
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
        public ActionResult ProduceListBySearch(string keyWord, string page, string sortBy)
        {
            ViewBag.CurrentUsername = GetUserInSession();

            int recordMax = 12;
            page = page ?? "page-1";
            string[] arrPage = page.Split('-');
            int numberPage = Convert.ToInt16(arrPage[1]) * recordMax;
            int numberPageActive = Convert.ToInt16(arrPage[1]);

            var models = new ProduceListBySearchModel();
            models.ListPosProduceBySearch = new List<PosProduceModel>();
            models.KeyWord = keyWord;

            models.ListPosCategory = (from cat in db.Loais
                                      select new PosCategoryModel
                                      {
                                          SeoFriendUrl = cat.maloai.ToString(),
                                          Name = cat.tenloai
                                      }).OrderBy(s => s.SeoFriendUrl).ToList();

            models.ListPosProduceLatest = (from pp in db.SanPhams
                                           join pc in db.Loais on pp.maloai equals pc.maloai
                                           where pp.ApprovedStatus == PosProduce_ApprovedStatus.Approved
                                           select new PosProduceModel
                                           {
                                               Guid = pp.masp,
                                               Name = pp.tensp,
                                               NameCategory = pc.tenloai,
                                               CategorySeoFriendUrl = pc.maloai.ToString(),
                                               SeoFriendUrl = pp.masp.ToString(),
                                               Price = pp.giaban,
                                               Unit = "VNĐ",
                                               ListGallery = (from fr in db.FProduceGalleries
                                                              join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                              where fr.GuidProduce == pp.masp
                                                              select new UGalleryModel
                                                              {
                                                                  Image = im.Image,
                                                                  CreatedAt = im.CreatedAt
                                                              }).OrderBy(x => x.CreatedAt).Take(1).ToList(),
                                               CreatedAt = pp.CreatedAt,
                                               Legit = pp.Legit,
                                               LegitCount = pp.LegitCount
                                           }).OrderBy(s => s.Guid).Take(9).ToList();

            foreach (var item in models.ListPosProduceLatest)
            {
                var tmpLinkImg = item.ListGallery;
                if (tmpLinkImg.Count == 0)
                {
                    item.LinkImg0 = "/Content/Custom/empty-album.png";
                }
                else
                {
                    item.LinkImg0 = tmpLinkImg[0].Image;
                }
            }

            if (!string.IsNullOrEmpty(keyWord))
            {
                var listPosProduceBySearch = from pp in db.SanPhams
                                             join pc in db.Loais on pp.maloai equals pc.maloai
                                             where pp.ApprovedStatus == PosProduce_ApprovedStatus.Approved
                                             && (pp.tensp.Contains(keyWord)
                                                || pp.Note.Contains(keyWord)
                                                || pp.mota.Contains(keyWord)
                                                || pp.CreatedBy.Contains(keyWord)
                                                || pc.tenloai.Contains(keyWord))
                                             select new PosProduceModel
                                             {
                                                 Guid = pp.masp,
                                                 Name = pp.tensp,
                                                 NameCategory = pc.tenloai,
                                                 CategorySeoFriendUrl = pc.maloai.ToString(),
                                                 SeoFriendUrl = pp.masp.ToString(),
                                                 Price = pp.giaban,
                                                 Unit = "VNĐ",
                                                 ListGallery = (from fr in db.FProduceGalleries
                                                                join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                                where fr.GuidProduce == pp.masp
                                                                select new UGalleryModel
                                                                {
                                                                    Image = im.Image,
                                                                    CreatedAt = im.CreatedAt
                                                                }).OrderBy(x => x.CreatedAt).Take(1).ToList(),
                                                 CreatedAt = pp.CreatedAt,
                                                 Legit = pp.Legit,
                                                 LegitCount = pp.LegitCount
                                             };

                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy)
                    {
                        case "newest":
                            listPosProduceBySearch = listPosProduceBySearch.OrderByDescending(x => x.CreatedAt);
                            break;
                        case "oldest":
                            listPosProduceBySearch = listPosProduceBySearch.OrderBy(x => x.CreatedAt);
                            break;
                        case "a-z":
                            listPosProduceBySearch = listPosProduceBySearch.OrderBy(x => x.Name);
                            break;
                        case "z-a":
                            listPosProduceBySearch = listPosProduceBySearch.OrderByDescending(x => x.Name);
                            break;
                        case "priceH-L":
                            listPosProduceBySearch = listPosProduceBySearch.OrderByDescending(x => x.Price);
                            break;
                        case "priceL-H":
                            listPosProduceBySearch = listPosProduceBySearch.OrderBy(x => x.Price);
                            break;
                        default:
                            listPosProduceBySearch = listPosProduceBySearch.OrderByDescending(x => x.CreatedAt);
                            break;
                    }
                }
                else
                {
                    listPosProduceBySearch = listPosProduceBySearch.OrderByDescending(x => x.CreatedAt);
                }

                //Sent data to view caculate
                ViewData["perpage"] = recordMax;
                ViewData["total"] = listPosProduceBySearch.Count();
                ViewData["pageActive"] = numberPageActive;

                //Check page start
                if (Convert.ToInt16(arrPage[1]) == 1)
                {
                    numberPage = 0;
                }
                else
                {
                    numberPage = numberPage - recordMax;
                }

                models.ListPosProduceBySearch = listPosProduceBySearch.Skip(numberPage).Take(recordMax).ToList();

                foreach (var item in models.ListPosProduceBySearch)
                {
                    var tmpLinkImg = item.ListGallery;
                    if (tmpLinkImg.Count == 0)
                    {
                        item.LinkImg0 = "/Content/Custom/empty-album.png";
                    }
                    else
                    {
                        item.LinkImg0 = tmpLinkImg[0].Image;
                    }
                }

                return View("~/Views/HomeCommon/ProduceListBySearch.cshtml", models);
            }
            else
            {
                //Sent data to view caculate
                ViewData["perpage"] = recordMax;
                ViewData["total"] = 0;
                ViewData["pageActive"] = numberPageActive;

                return View("~/Views/HomeCommon/ProduceListBySearch.cshtml", models);
            }
        }

        [ChildActionOnly]
        public ActionResult Header()
        {
            ViewBag.CurrentUsername = GetUserInSession();

            var model = new HeaderModel();
            model.ListMenu = new List<SMenuModel>();
            List<SMenuModel> listSMenuModel = new List<SMenuModel>();

            listSMenuModel.Add(new SMenuModel("/danh-muc-tin-tuc", "Tin tức", 4));
            listSMenuModel.Add(new SMenuModel("/gioi-thieu", "Giới thiệu", 2));
            listSMenuModel.Add(new SMenuModel("/lien-he", "Liên hệ", 5));
            listSMenuModel.Add(new SMenuModel("/", "Trang chủ", 1));
            listSMenuModel.Add(new SMenuModel("/su-menh", "Sứ mệnh", 3));
            listSMenuModel = listSMenuModel.OrderBy(t => t.OrderMenu).ToList();
            try
            {
                model.ListMenu = listSMenuModel;

                var username = GetUserInSession();
                model.CurrentUsername = String.IsNullOrWhiteSpace(username) ? "Đăng nhập" : username;
            }
            catch (Exception ex)
            {
                model.ListMenu = new List<SMenuModel>();
            }

            return PartialView(model);
        }
        [ChildActionOnly]
        public ActionResult Footer()
        {
            var model = new FooterModel();
            model.ListPosCategory = new List<PosCategoryModel>();

            try
            {
                model.ListPosCategory = (from cat in db.Loais
                                         select new PosCategoryModel
                                         {
                                             SeoFriendUrl = cat.maloai.ToString(),
                                             Name = cat.tenloai
                                         }).OrderBy(s => s.Name).ToList();
            }
            catch (Exception)
            {
                model.ListPosCategory = new List<PosCategoryModel>();
            }

            return PartialView(model);
        }
        #region Contact Us

        public ActionResult ContactUs()
        {
            var models = new ContactUsModel();
            
            models.ListPosCategory = (from cat in db.Loais
                                      select new PosCategoryModel
                                      {
                                          SeoFriendUrl = cat.maloai.ToString(),
                                          Name = cat.tenloai
                                      }).ToList();

            return View(models);
        }

        #endregion
        #region Introduce

        public ActionResult Introduce()
        {
            var models = new ContactUsModel();

            models.ListPosCategory = (from cat in db.Loais
                                      select new PosCategoryModel
                                      {
                                          SeoFriendUrl = cat.maloai.ToString(),
                                          Name = cat.tenloai
                                      }).ToList();

            return View(models);
        }

        #endregion
        #region Mission

        public ActionResult Mission()
        {
            var models = new ContactUsModel();

            models.ListPosCategory = (from cat in db.Loais
                                      select new PosCategoryModel
                                      {
                                          SeoFriendUrl = cat.maloai.ToString(),
                                          Name = cat.tenloai
                                      }).ToList();

            return View(models);
        }

        #endregion

        #region Home
        public ActionResult PartialLatestProduct()
        {
            ViewBag.CurrentUsername = GetUserInSession();

            var model = new PartialLatestProductModel();

            model.ListPosProduce = (from pp in db.SanPhams
                                    join pc in db.Loais on pp.maloai equals pc.maloai
                                    where pp.ApprovedStatus==2
                                    select new PosProduceModel
                                    {
                                        Guid = pp.masp,
                                        Name = pp.tensp,
                                        NameCategory = pc.tenloai,
                                        CategorySeoFriendUrl = pc.maloai.ToString(),
                                        SeoFriendUrl = pp.masp.ToString(),
                                        Price = pp.giaban,
                                        Unit = "VNĐ",
                                        ListGallery = (from fr in db.FProduceGalleries
                                                       join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                       where fr.GuidProduce == pp.masp
                                                       select new UGalleryModel
                                                       {
                                                           Image = im.Image,
                                                           CreatedAt = im.CreatedAt
                                                       }).OrderBy(x => x.CreatedAt).Take(1).ToList(),
                                        CreatedAt = pp.CreatedAt,
                                        CreatedBy = pp.CreatedBy,
                                        Legit = pp.Legit,
                                        LegitCount = pp.LegitCount
                                    }).OrderByDescending(s => s.Guid).Take(20).ToList();

            foreach (var item in model.ListPosProduce)
            {
                var tmpLinkImg = item.ListGallery;
                if (tmpLinkImg.Count == 0)
                {
                    item.LinkImg0 = "/Content/Custom/empty-album.png";
                }
                else
                {
                    item.LinkImg0 = tmpLinkImg[0].Image;
                }

            }

            if (model.ListPosProduce != null && model.ListPosProduce.Any())
            {
                model.ListPosCategory = model.ListPosProduce.GroupBy(x => new { x.NameCategory, x.CategorySeoFriendUrl }).Select(x => new PosCategoryModel()
                {
                    Name = x.Key.NameCategory,
                    SeoFriendUrl = x.Key.CategorySeoFriendUrl
                }).OrderBy(x => x.Name).ToList();
            }

            return PartialView(model);
        }
        public ActionResult PartialFeaturedProduct()
        {
            var model = new PartialFeaturedProductModel();

            model.ListPosProduce = (from pp in db.SanPhams
                                    join pc in db.Loais on pp.maloai equals pc.maloai
                                    select new PosProduceModel
                                    {
                                        Guid = pp.masp,
                                        Name = pp.tensp,
                                        NameCategory = pc.tenloai,
                                        CategorySeoFriendUrl = pc.maloai.ToString(),
                                        SeoFriendUrl = pp.masp.ToString(),
                                        Price = pp.giaban,
                                        Unit = "VNĐ",
                                        ListGallery = (from fr in db.FProduceGalleries
                                                       join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                       where fr.GuidProduce == pp.masp
                                                       select new UGalleryModel
                                                       {
                                                           Image = im.Image,
                                                           CreatedAt = im.CreatedAt
                                                       }).OrderBy(x => x.CreatedAt).Take(1).ToList(),
                                    
                                        CreatedAt = pp.CreatedAt,
                                        Legit = pp.Legit,
                                        LegitCount = pp.LegitCount
                                    }).OrderByDescending(s => s.Guid).Take(20).ToList();

            foreach (var item in model.ListPosProduce)
            {
                var tmpLinkImg = item.ListGallery;
                if (tmpLinkImg.Count == 0)
                {
                    item.LinkImg0 = "/Content/Custom/empty-album.png";
                }
                else
                {
                    item.LinkImg0 = tmpLinkImg[0].Image;
                }
            }

            return PartialView(model);
        }

        #endregion
        #region Produce List By Category

        public ActionResult ProduceListByCategory(string seoFriendUrl, string page, string sortBy)
        {
            ViewBag.CurrentUsername = GetUserInSession();

            if (!string.IsNullOrEmpty(seoFriendUrl))
            {
                int recordMax = 12;
                page = page ?? "page-1";
                string[] arrPage = page.Split('-');
                int numberPage = Convert.ToInt16(arrPage[1]) * recordMax;
                int numberPageActive = Convert.ToInt16(arrPage[1]);

                var models = new ProduceListByCategoryModel();

                models.PosCategory = new PosCategoryModel();
                models.ListPosCategory = new List<PosCategoryModel>();
                models.ListPosProduceLatest = new List<PosProduceModel>();
                models.ListPosProduceByCategory = new List<PosProduceModel>();

                models.ListPosCategory = (from cat in db.Loais
                                          select new PosCategoryModel
                                          {
                                              SeoFriendUrl = cat.maloai.ToString(),
                                              Name = cat.tenloai
                                          }).OrderBy(s => s.Name).ToList();

                models.ListPosProduceLatest = (from pp in db.SanPhams
                                               join pc in db.Loais on pp.maloai equals pc.maloai
                                               select new PosProduceModel
                                               {
                                                   Guid = pp.masp,
                                                   Name = pp.tensp,
                                                   NameCategory = pc.tenloai,
                                                   CategorySeoFriendUrl = pc.maloai.ToString(),
                                                   SeoFriendUrl = pp.masp.ToString(),
                                                   Price = pp.giaban,
                                                   Unit = "VNĐ",
                                                   ListGallery = (from fr in db.FProduceGalleries
                                                                  join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                                  where fr.GuidProduce == pp.masp
                                                                  select new UGalleryModel
                                                                  {
                                                                      Image = im.Image,
                                                                      CreatedAt = im.CreatedAt
                                                                  }).OrderBy(x => x.CreatedAt).Take(1).ToList(),

                                                   CreatedAt = pp.ngaycapnhat,
                                                   CreatedBy = pp.CreatedBy,
                                                   FullAddress = null,
                                                   Legit = pp.Legit,
                                                   LegitCount = pp.LegitCount
                                               }).OrderByDescending(s => s.Guid).Take(9).ToList();

                foreach (var item in models.ListPosProduceLatest)
                {
                    var tmpLinkImg = item.ListGallery;
                    if (tmpLinkImg.Count == 0)
                    {
                        item.LinkImg0 = "/Content/Custom/empty-album.png";
                    }
                    else
                    {
                        item.LinkImg0 = tmpLinkImg[0].Image;
                    }
                }

                models.PosCategory = (from cat in db.Loais
                                      where cat.maloai.ToString() == seoFriendUrl
                                      select new PosCategoryModel
                                      {
                                          Guid = cat.maloai.ToString(),
                                          SeoFriendUrl = cat.maloai.ToString(),
                                          Name = cat.tenloai
                                      }).FirstOrDefault();

                if (models.PosCategory != null)
                {
                    var listPosProduceByCategory = from pp in db.SanPhams
                                                   join pc in db.Loais on pp.maloai equals pc.maloai
                                                   where pp.maloai.ToString() == models.PosCategory.Guid
                                                   select new PosProduceModel
                                                   {
                                                       Guid = pp.masp,
                                                       Name = pp.tensp,
                                                       NameCategory = pc.tenloai,
                                                       CategorySeoFriendUrl = pc.maloai.ToString(),
                                                       SeoFriendUrl = pp.masp.ToString(),
                                                       Price = pp.giaban,
                                                       Unit = "VNĐ",
                                                       ListGallery = (from fr in db.FProduceGalleries
                                                                      join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                                      where fr.GuidProduce == pp.masp
                                                                      select new UGalleryModel
                                                                      {
                                                                          Image = im.Image,
                                                                          CreatedAt = im.CreatedAt
                                                                      }).OrderBy(x => x.CreatedAt).Take(1).ToList(),

                                                       CreatedAt = pp.ngaycapnhat,
                                                       CreatedBy = pp.CreatedBy,
                                                       FullAddress = null,
                                                       Legit = pp.Legit,
                                                       LegitCount = pp.LegitCount
                                                   };

                    if (!string.IsNullOrEmpty(sortBy))
                    {
                        switch (sortBy)
                        {
                            case "newest":
                                listPosProduceByCategory = listPosProduceByCategory.OrderByDescending(x => x.CreatedAt);
                                break;
                            case "oldest":
                                listPosProduceByCategory = listPosProduceByCategory.OrderBy(x => x.CreatedAt);
                                break;
                            case "a-z":
                                listPosProduceByCategory = listPosProduceByCategory.OrderBy(x => x.Name);
                                break;
                            case "z-a":
                                listPosProduceByCategory = listPosProduceByCategory.OrderByDescending(x => x.Name);
                                break;
                            case "priceH-L":
                                listPosProduceByCategory = listPosProduceByCategory.OrderByDescending(x => x.Price);
                                break;
                            case "priceL-H":
                                listPosProduceByCategory = listPosProduceByCategory.OrderBy(x => x.Price);
                                break;
                            default:
                                listPosProduceByCategory = listPosProduceByCategory.OrderByDescending(x => x.CreatedAt);
                                break;
                        }
                    }
                    else
                    {
                        listPosProduceByCategory = listPosProduceByCategory.OrderByDescending(x => x.CreatedAt);
                    }

                    //Sent data to view caculate
                    ViewData["perpage"] = recordMax;
                    ViewData["total"] = listPosProduceByCategory.Count();
                    ViewData["pageActive"] = numberPageActive;

                    //Check page start
                    if (Convert.ToInt16(arrPage[1]) == 1)
                    {
                        numberPage = 0;
                    }
                    else
                    {
                        numberPage = numberPage - recordMax;
                    }

                    models.ListPosProduceByCategory = listPosProduceByCategory.Skip(numberPage).Take(recordMax).ToList();

                    foreach (var item in models.ListPosProduceByCategory)
                    {
                        var tmpLinkImg = item.ListGallery;
                        if (tmpLinkImg.Count == 0)
                        {
                            item.LinkImg0 = "/Content/Custom/empty-album.png";
                        }
                        else
                        {
                            item.LinkImg0 = tmpLinkImg[0].Image;
                        }

                    }

                    return View("~/Views/HomeCommon/ProduceListByCategory.cshtml", models);
                }
                else
                {
                    return View("~/Views/HomeCommon/Error_404.cshtml");
                }
            }

            return View("~/Views/HomeCommon/Error_404.cshtml");
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
                                       IsAdmin = p.tenrole=="admin"?true:false,
                                       Avartar = null
                                   }).FirstOrDefault();
            if (userInformation != null)
            {
                ViewBag.Pagesize = (userInformation.RecordsInPage == null || userInformation.RecordsInPage == 0) ? 10 : userInformation.RecordsInPage;
                return userInformation;
            }
            return new SUserModel();
        }
        #endregion
        [HttpPost]
        public ActionResult PayCart(string fullAddress, string mobile)
        {
            string msg = "";
            int statusCode = (int)Convert.ToInt16(HttpStatusCode.Conflict);
            try
            {
                var username = GetUserInSession();
                var user = db.KhachHangs.FirstOrDefault(x => x.tendangnhap == username);
                if (user != null)
                {
                    var cookieName = "cartProduce_" + user.tendangnhap;
                    if (Request.Cookies[cookieName] != null)
                    {
                        var cookieData = string.IsNullOrEmpty(Request.Cookies[cookieName].Value) ? String.Empty : Request.Cookies[cookieName].Value;
                        var lstProduce = JsonConvert.DeserializeObject<List<ProduceCartCookieModel>>(cookieData);

                        if (lstProduce != null && lstProduce.Any())
                        {
                            var lstGuidProduce = string.Join(",", lstProduce.Select(x => x.GuidProduce));
                            var listPosProduceCart = (from pp in db.SanPhams
                                                      join pc in db.SanPhams on pp.maloai equals pc.maloai
                                                      select new PosProduceModel
                                                      {
                                                          Guid = pp.masp,
                                                          Price = pp.giaban,
                                                      }).ToList();

                            listPosProduceCart = listPosProduceCart.Where(t => lstGuidProduce.Contains(t.Guid.ToString())).ToList();
                            //Get payment input
                            DonHang order = new DonHang();
                            //Save order to db
                            order.madon = db.DonHangs.Count();
                            order.makh = user.makh;
                            order.diachi = fullAddress;
                            order.dienthoai = mobile;
                            order.ngaydat = DateTime.Now;
                            db.DonHangs.InsertOnSubmit(order);
                            db.SubmitChanges();

                            List<ChiTietDonHang> orderDetails = new List<ChiTietDonHang>();
                            foreach (var item in listPosProduceCart)
                            {
                                orderDetails.Add(new ChiTietDonHang()
                                {
                       
                                    madon = order.madon,
                                    masp = item.Guid,
                                    soluong = (lstProduce.FirstOrDefault(x => x.GuidProduce == item.Guid.ToString().ToLower().Trim())?.Quantity).GetValueOrDefault(0),
                                    gia = item.Price,
                                    trangthai = 0
                                });
                            }
                            db.ChiTietDonHangs.InsertAllOnSubmit(orderDetails);

                            db.SubmitChanges();

                            statusCode = (int)Convert.ToInt16(HttpStatusCode.OK);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return Json(new { StatusCode = statusCode, Message = msg }, "text/plain");
        }

        public ActionResult ProduceDetail(string seoFriendUrl)
        {
            ViewBag.CurrentUsername = GetUserInSession();

            if (!string.IsNullOrEmpty(seoFriendUrl))
            {
                var models = new ProduceDetailModel();

                models.SUser = GetSettingUser();

                models.ListPosCategory = (from cat in db.Loais
                                          select new PosCategoryModel
                                          {
                                              SeoFriendUrl = cat.maloai.ToString(),
                                              Name = cat.tenloai,
                                          }).OrderBy(s => s.Name).ToList();

                models.PosProduce = (from pp in db.SanPhams
                                     join pc in db.Loais on pp.maloai equals pc.maloai
                                     where  pp.masp.ToString() == seoFriendUrl
                                     select new PosProduceModel
                                     {
                                         Guid = pp.masp,
                                         Name = pp.tensp,
                                         ListGallery = (from fr in db.FProduceGalleries
                                                        join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                        where fr.GuidProduce == pp.masp
                                                        select new UGalleryModel
                                                        {
                                                            Image = im.Image
                                                        }).ToList(),
                                         Legit = pp.Legit,
                                         LegitCount = pp.LegitCount,
                                         Price = pp.giaban,
                                         Unit = "VNĐ",
                                         Note = pp.Note,
                                         Description = pp.mota,
                                         GuidCategory = (int)pp.maloai,
                                         CreatedBy = pp.CreatedBy,
                                         NameCategory = pc.tenloai,
                                         CategorySeoFriendUrl = pc.maloai.ToString(),
                                       }).FirstOrDefault();

                if (models.PosProduce != null)
                {
                    models.PosProduceCreatedBy = (from k in db.KhachHangs where k.tendangnhap == models.PosProduce.CreatedBy select 
                                                  new SUser { 
                                                      Guid=k.makh,
                                                      FullName=k.hoten
                                                     
                                                  }).FirstOrDefault();
                    if (models.PosProduceCreatedBy!=null && string.IsNullOrEmpty(models.PosProduceCreatedBy.Avartar))
                    {
                        models.PosProduceCreatedBy.Avartar = "/Content/UserFiles/Cauhinh/ImageEmpty.jpg";
                    }
                    else if (models.PosProduceCreatedBy == null)
                    {
                        models.PosProduceCreatedBy = new SUser();
                        models.PosProduceCreatedBy.Avartar = "/Content/UserFiles/Cauhinh/ImageEmpty.jpg";
                    }


                    models.ListProduceSameCreatedBy = (from pp in db.SanPhams
                                                       join pc in db.Loais on pp.maloai equals pc.maloai
                                                       where pp.masp != models.PosProduce.Guid
                                                       select new PosProduceModel
                                                       {
                                                           Guid = pp.masp,
                                                           Name = pp.tensp,
                                                           NameCategory = pc.tenloai,
                                                           CategorySeoFriendUrl = pc.maloai.ToString(),
                                                           SeoFriendUrl = pp.masp.ToString(),
                                                           Price = pp.giaban,
                                                           Unit = "VNĐ",
                                                           ListGallery = (from fr in db.FProduceGalleries
                                                                          join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                                          where fr.GuidProduce == pp.masp
                                                                          select new UGalleryModel
                                                                          {
                                                                              Image = im.Image,
                                                                              CreatedAt = im.CreatedAt
                                                                          }).OrderBy(x => x.CreatedAt).Take(1).ToList(),
                                                      
                                                           CreatedAt = pp.ngaycapnhat,
                                                           CreatedBy = pp.CreatedBy,
                                                           FullAddress = null,
                                                           Legit = pp.Legit,
                                                           LegitCount = pp.LegitCount
                                                       }).OrderByDescending(s => s.Guid).Take(4).ToList();

                    foreach (var item in models.ListProduceSameCreatedBy)
                    {
                        var tmpLinkImg = item.ListGallery;
                        if (tmpLinkImg.Count == 0)
                        {
                            item.LinkImg0 = "/Content/Custom/empty-album.png";
                        }
                        else
                        {
                            item.LinkImg0 = tmpLinkImg[0].Image;
                        }
                    }

                    models.ListProduceSameCategory = (from pp in db.SanPhams
                                                      join pc in db.Loais on pp.maloai equals pc.maloai
                                                      where pp.maloai == models.PosProduce.GuidCategory && pp.masp != models.PosProduce.Guid
                                                      select new PosProduceModel
                                                      {
                                                          Guid = pp.masp,
                                                          Name = pp.tensp,
                                                          NameCategory = pc.tenloai,
                                                          CategorySeoFriendUrl = pc.maloai.ToString(),
                                                          SeoFriendUrl = pp.masp.ToString(),
                                                          Price = pp.giaban,
                                                          Unit = "VNĐ",
                                                          ListGallery = (from fr in db.FProduceGalleries
                                                                         join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                                         where fr.GuidProduce == pp.masp
                                                                         select new UGalleryModel
                                                                         {
                                                                             Image = im.Image,
                                                                             CreatedAt = im.CreatedAt
                                                                         }).OrderBy(x => x.CreatedAt).Take(1).ToList(),

                                                          CreatedAt = pp.ngaycapnhat,
                                                          CreatedBy = pp.CreatedBy,
                                                          FullAddress = null,
                                                          Legit = pp.Legit,
                                                          LegitCount = pp.LegitCount
                                                      }).OrderByDescending(s => s.Guid).Take(4).ToList();

                    foreach (var item in models.ListProduceSameCategory)
                    {
                        var tmpLinkImg = item.ListGallery;
                        if (tmpLinkImg.Count == 0)
                        {
                            item.LinkImg0 = "/Content/Custom/empty-album.png";
                        }
                        else
                        {
                            item.LinkImg0 = tmpLinkImg[0].Image;
                        }
                    }

                    models.NewRatingProduce = new WRatingProduceModel()
                    {
                        GuidProduce = models.PosProduce.Guid
                    };

                    return View("~/Views/HomeCommon/ProduceDetail.cshtml", models);
                }
                else
                {
                    return View("~/Views/HomeCommon/Error_404.cshtml");
                }
            }

            return View("~/Views/HomeCommon/Error_404.cshtml");
        }
        #region News List

        public ActionResult NewsList(string page)
        {
            int recordMax = 9;
            page = page ?? "page-1";
            string[] arrPage = page.Split('-');
            int numberPage = Convert.ToInt16(arrPage[1]) * recordMax;
            int numberPageActive = Convert.ToInt16(arrPage[1]);

            var models = new NewsListModel();
            models.ListNews = new List<UNewsModel>();

            models.ListPosCategory = (from cat in db.Loais
                                      select new PosCategoryModel
                                      {
                                          SeoFriendUrl = cat.maloai.ToString(),
                                          Name = cat.tenloai
                                      }).OrderBy(s => s.SeoFriendUrl).ToList();

            var listNews = (from un in db.UNews
                            join su in db.KhachHangs on un.CreatedBy equals su.tendangnhap
                            where un.Active==true && un.Status == 1
                            select new UNewsModel
                            {
                                Guid = un.Guid,
                                CreatedAt = un.CreatedAt,
                                CreatedBy = un.CreatedBy,
                                CreatedByFullName = su.hoten,
                                ContentNews = un.ContentNews,
                                Name = un.Name,
                                Note = un.Note,
                                SeoFriendUrl = un.SeoFriendUrl,
                                UrlImageFeatured = "/Content/UserFiles/Cauhinh/ImageEmpty.jpg"
                            }).OrderByDescending(x => x.CreatedAt);

            //Sent data to view caculate
            ViewData["perpage"] = recordMax;
            ViewData["total"] = listNews.Count();
            ViewData["pageActive"] = numberPageActive;

            //Check page start
            if (Convert.ToInt16(arrPage[1]) == 1)
            {
                numberPage = 0;
            }
            else
            {
                numberPage = numberPage - recordMax;
            }

            models.ListNews = listNews.Skip(numberPage).Take(recordMax).ToList();

            foreach (var item in models.ListNews)
            {
                try
                {
                    var content = HttpUtility.HtmlDecode(item.ContentNews);
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(content);

                    if (doc.DocumentNode.SelectSingleNode("//img") != null)
                    {
                        item.UrlImageFeatured = doc.DocumentNode.SelectSingleNode("//img").Attributes["src"].Value;
                    }

                }
                catch
                {
                    item.UrlImageFeatured = "/Content/UserFiles/Cauhinh/ImageEmpty.jpg";
                }
            }

            return View("~/Views/HomeCommon/NewsList.cshtml", models);
        }

        #endregion

        #region News Detail

        public ActionResult NewsDetail(string seoFriendUrl)
        {
            if (!string.IsNullOrEmpty(seoFriendUrl))
            {
                var models = new NewsDetailModel();

                models.ListPosCategory = (from cat in db.Loais
                                          select new PosCategoryModel
                                          {
                                              SeoFriendUrl = cat.maloai.ToString(),
                                              Name = cat.tenloai
                                          }).OrderBy(s => s.SeoFriendUrl).ToList();

                models.UNews = (from un in db.UNews
                                join su in db.KhachHangs on un.CreatedBy equals su.tendangnhap
                                where un.Active==true && un.Status == UNews_Status.Approved && un.Guid.ToString().ToLower().Equals(seoFriendUrl)
                                select new UNewsModel
                                {
                                    Guid = un.Guid,
                                    CreatedAt = un.CreatedAt,
                                    CreatedBy = un.CreatedBy,
                                    CreatedByFullName = su.hoten,
                                    ContentNews = un.ContentNews,
                                    Name = un.Name,
                                    Note = un.Note,
                                    SeoFriendUrl = un.SeoFriendUrl,
                                    UrlImageFeatured = "/Content/UserFiles/Cauhinh/ImageEmpty.jpg"
                                }).FirstOrDefault();

                if (models.UNews != null)
                {
                    models.ListNewsSameCreatedBy = (from un in db.UNews
                                                    join su in db.KhachHangs on un.CreatedBy equals su.tendangnhap
                                                    where un.Active==true && un.Status == UNews_Status.Approved && un.CreatedBy == models.UNews.CreatedBy && un.Guid != models.UNews.Guid
                                                    select new UNewsModel
                                                    {
                                                        Guid = un.Guid,
                                                        CreatedAt = un.CreatedAt,
                                                        CreatedBy = un.CreatedBy,
                                                        CreatedByFullName = su.hoten,
                                                        ContentNews = un.ContentNews,
                                                        Name = un.Name,
                                                        Note = un.Note,
                                                        SeoFriendUrl = un.SeoFriendUrl,
                                                        UrlImageFeatured = "/Content/UserFiles/Cauhinh/ImageEmpty.jpg"
                                                    }).OrderByDescending(s => s.CreatedAt).Take(3).ToList();

                    foreach (var item in models.ListNewsSameCreatedBy)
                    {
                        try
                        {
                            var content = HttpUtility.HtmlDecode(item.ContentNews);
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(content);
                            if (doc.DocumentNode.SelectSingleNode("//img") != null)
                            {
                                var link = doc.DocumentNode.SelectSingleNode("//img").Attributes["src"].Value;

                                item.UrlImageFeatured = link;
                            }

                        }
                        catch
                        {
                            item.UrlImageFeatured = "/Content/UserFiles/Cauhinh/ImageEmpty.jpg";
                        }
                    }

                    return View("~/Views/HomeCommon/NewsDetail.cshtml", models);
                }
                else
                {
                    return View("~/Views/HomeCommon/Error_404.cshtml");
                }
            }

            return View("~/Views/HomeCommon/Error_404.cshtml");
        }

        #endregion

        [HttpGet]
        public ActionResult ProduceCart()
        {
            var username = GetUserInSession();
            ViewBag.CurrentUsername = username;

            if (string.IsNullOrEmpty(username))
            {
                return View("~/Views/HomeCommon/Error_404.cshtml");
            }
            else
            {
                var models = new ProduceCartModel();
                models.ListPosProduceCart = new List<PosProduceModel>();

                models.ListPosCategory = (from cat in db.Loais
                                          select new PosCategoryModel
                                          {
                                              SeoFriendUrl = cat.maloai.ToString(),
                                              Name = cat.tenloai
                                          }).OrderBy(s => s.SeoFriendUrl).ToList();

                var cookieName = "cartProduce_" + username;
                if (Request.Cookies[cookieName] != null)
                {
                    var cookieData = string.IsNullOrEmpty(Request.Cookies[cookieName].Value) ? String.Empty : Request.Cookies[cookieName].Value;
                    var lstProduce = JsonConvert.DeserializeObject<List<ProduceCartCookieModel>>(cookieData);
                    if (lstProduce != null && lstProduce.Any())
                    {
                        var lstGuidProduce = string.Join(",", lstProduce.Select(x => x.GuidProduce));
                        var listPosProduceCart = (from pp in db.SanPhams
                                                  join pc in db.Loais on pp.maloai equals pc.maloai
                                                  //where lstGuidProduce.Contains(pp.masp.ToString().ToLower().Trim())
                                                  select new PosProduceModel
                                                  {
                                                      Guid = pp.masp,
                                                      Code = pp.masp.ToString(),
                                                      Name = pp.tensp,
                                                      NameCategory = pc.tenloai,
                                                      CategorySeoFriendUrl = pc.maloai.ToString(),
                                                      SeoFriendUrl = pp.masp.ToString(),
                                                      Price = pp.giaban,
                                                      Unit = "VNĐ",
                                                      ListGallery = (from fr in db.FProduceGalleries
                                                                     join im in db.UGalleries on fr.GuidGallery equals im.Guid
                                                                     where fr.GuidProduce == pp.masp
                                                                     select new UGalleryModel
                                                                     {
                                                                         Image = im.Image,
                                                                         CreatedAt = im.CreatedAt
                                                                     }).OrderBy(x => x.CreatedAt).Take(1).ToList(),
                                                      CreatedAt = pp.CreatedAt,
                                                      Legit = pp.Legit,
                                                      LegitCount = pp.LegitCount,
                                                  }).ToList();

                        listPosProduceCart = listPosProduceCart.Where(t => lstGuidProduce.Contains(t.Guid.ToString().ToLower().Trim())).ToList();
                        foreach (var item in listPosProduceCart)
                        {
                            item.Quantity = lstProduce.FirstOrDefault(x => x.GuidProduce == item.Guid.ToString().ToLower().Trim())?.Quantity;

                            var tmpLinkImg = item.ListGallery;
                            if (tmpLinkImg.Count == 0)
                            {
                                item.LinkImg0 = "/Content/Custom/empty-album.png";
                            }
                            else
                            {
                                item.LinkImg0 = tmpLinkImg[0].Image;
                            }
                        }

                        models.ListPosProduceCart = listPosProduceCart.Where(x => x.Quantity > 0).ToList();

                        models.ListTotalByUnit = new Dictionary<string, decimal>();
                        foreach (var itemGroup in models.ListPosProduceCart.GroupBy(x => x.Unit))
                        {
                            models.ListTotalByUnit.Add(itemGroup.Key, itemGroup.Sum(x => x.Price.GetValueOrDefault(0) * x.Quantity.GetValueOrDefault(0)));
                        }

                        return View("~/Views/HomeCommon/ProduceCart.cshtml", models);
                    }
                    else
                    {
                        return View("~/Views/HomeCommon/ProduceCart.cshtml", models);
                    }
                }
                else
                {
                    return View("~/Views/HomeCommon/Error_404.cshtml");
                }
            }
        }
    }
}