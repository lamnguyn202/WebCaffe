using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebCaffe.Models;

namespace WebCaffe.Controllers
{
    public class HomeController : Controller
    {
        CaffeDataContext db=new CaffeDataContext();
        public ActionResult Index()
        {
            HomeModel model = new HomeModel();
            model.ListPosCategory = new List<PosCategoryModel>();

            model.ListPosCategory = (from cat in db.Loais 
                                     select new PosCategoryModel
                                     {
                                         SeoFriendUrl = cat.maloai.ToString(),
                                         Name = cat.tenloai
                                     }).ToList();

            return View("Index", model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}