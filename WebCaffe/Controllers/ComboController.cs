using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebCaffe.Models;

namespace WebCaffe.Controllers
{
    public class ComboController : Controller
    {
        CaffeDataContext db=new CaffeDataContext();
        // GET: Combo
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult SRole()
        {
            IEnumerable<Role> sMenus = db.Roles.OrderBy(p => p.tenrole);
            return Json(sMenus.Select(c => new { c.marole, c.tenrole }), JsonRequestBehavior.AllowGet);
        }
        public JsonResult PosCategory()
        {
            List<Loai> list= db.Loais.OrderBy(p => p.maloai).ToList();

            return Json(list.Select(c => new {Guid= c.maloai, Name = c.tenloai }), JsonRequestBehavior.AllowGet);
        }
        public JsonResult SType(string key)
        {
            List<SType> listType = new List<SType>();
            listType.Add( new SType("100", "100"));
            listType.Add( new SType("500", "500"));
            listType.Add( new SType("1000", "1000"));
            listType.Add( new SType("200", "200"));


            IEnumerable<SType> sTypes = listType.OrderBy(p => p.ValueType);
            return Json(sTypes.Select(c => new { c.ValueType, c.Note }), JsonRequestBehavior.AllowGet);
        }
        
    }
}