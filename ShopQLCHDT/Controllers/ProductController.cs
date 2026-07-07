using ShopQLCHDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace ShopQLCHDT.Controllers
{
    public class ProductController : Controller
    {
        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();

        // 1. Danh sách sản phẩm (có lọc Thương hiệu & Tìm kiếm)
        public ActionResult Index(int? math, string keyword)
        {
            var products = db.SANPHAM.Include(s => s.THUONGHIEU).AsQueryable();


            products = products.Where(p => p.TRANGTHAI == "Đang kinh doanh");

            if (math.HasValue)
                products = products.Where(p => p.MATH == math.Value);

            if (!string.IsNullOrEmpty(keyword))
                products = products.Where(p => p.TENSP.Contains(keyword));

            return View(products.ToList());
        }

        // 2. Chi tiết sản phẩm
        public ActionResult Details(int id)
        {
            var sp = db.SANPHAM.Find(id);
            if (sp == null) return HttpNotFound();


            var tonKho = db.KHO_SANPHAM.FirstOrDefault(k => k.MASP == id);
            ViewBag.SoLuongTon = tonKho != null ? tonKho.SOLUONGTON : 0;

            return View(sp);
        }
    }
}
