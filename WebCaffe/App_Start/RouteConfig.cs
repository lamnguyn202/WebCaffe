using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebCaffe.Models;

namespace WebCaffe
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            try
            {
                routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

                #region Add portal admin in router
                var db = new CaffeDataContext();

                routes.MapRoute(
                    name: "admin",
                    url: "admin" + "/{controller}/{action}/{id}/{Menu}",
                    defaults: new { Portal = "admin", controller = "Admin", action = "Index", id = UrlParameter.Optional, Menu = "start" }
                );
                #endregion

                #region Web
                routes.MapRoute(
                    name: "CuaHangDangTheoDoi",
                    url: "theo-doi",
                    defaults: new { controller = "HomeCommon", action = "UserListByFollow" }
                );

                routes.MapRoute(
                    name: "TinDaLuu",
                    url: "tin-da-luu",
                    defaults: new { controller = "HomeCommon", action = "ProduceListByLoved" }
                );

                routes.MapRoute(
                    name: "GioHang",
                    url: "gio-hang",
                    defaults: new { controller = "HomeCommon", action = "ProduceCart" }
                );

                routes.MapRoute(
                    name: "TimKiem",
                    url: "tim-kiem",
                    defaults: new { controller = "HomeCommon", action = "ProduceListBySearch" }
                );

                routes.MapRoute(
                    name: "CuaHang",
                    url: "cua-hang/{createdBy}",
                    defaults: new { controller = "HomeCommon", action = "ProduceListByCreatedBy", createdBy = UrlParameter.Optional }
                );

                routes.MapRoute(
                    name: "DanhMucSanPham",
                    url: "danh-muc/{seoFriendUrl}",
                    defaults: new { controller = "HomeCommon", action = "ProduceListByCategory", seoFriendUrl = UrlParameter.Optional }
                );

                routes.MapRoute(
                    name: "ChiTietSanPham",
                    url: "san-pham/{seoFriendUrl}",
                    defaults: new { controller = "HomeCommon", action = "ProduceDetail", seoFriendUrl = UrlParameter.Optional }
                );

                routes.MapRoute(
                    name: "DanhMucTinTuc",
                    url: "danh-muc-tin-tuc",
                    defaults: new { controller = "HomeCommon", action = "NewsList" }
                );
                routes.MapRoute(
                    name: "GioiThieu",
                    url: "gioi-thieu",
                    defaults: new { controller = "HomeCommon", action = "Introduce" }
                );
                routes.MapRoute(
                    name: "SuMenh",
                    url: "su-menh",
                    defaults: new { controller = "HomeCommon", action = "Mission" }
                );

                routes.MapRoute(
                    name: "ChiTietTinTuc",
                    url: "tin-tuc/{seoFriendUrl}",
                    defaults: new { controller = "HomeCommon", action = "NewsDetail", seoFriendUrl = UrlParameter.Optional }
                );

                routes.MapRoute(
                    name: "LienHe",
                    url: "lien-he",
                    defaults: new { controller = "HomeCommon", action = "ContactUs" }
                );

                #endregion

                #region Router defaults
                //==================================================================//
                // Router cho trang chu va cac PartialView
                routes.MapRoute(
                    name: "Default",
                    url: "{controller}/{action}/{id}/{id1}",
                    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, id1 = UrlParameter.Optional }
                );
                #endregion
            }
            catch (Exception ex)
            {

            }
        }
    }
}