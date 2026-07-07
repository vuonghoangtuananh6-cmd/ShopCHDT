using ShopQLCHDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopQLCHDT.Controllers
{
    public class HomeController : Controller
    {
        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();
        public ActionResult Index()
        {
            
            var sanPhams = db.SANPHAM
                             .Where(s => s.TRANGTHAI == "Đang kinh doanh")
                             .OrderBy(s => s.MASP)
                             .Take(8)
                             .ToList();
            return View(sanPhams);
        }
    }
}