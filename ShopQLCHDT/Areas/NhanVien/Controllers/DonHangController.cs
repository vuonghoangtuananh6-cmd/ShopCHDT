using ShopQLCHDT.Filters;
using ShopQLCHDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace ShopQLCHDT.Areas.NhanVien.Controllers
{
    public class DonHangController : Controller
    {
        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();

        //
        // GET: NhanVien/DonHang
        //
        public ActionResult Index(string trangThai = "")
        {
            var hoadons = db.HOADON
                            .Include(h => h.KHACHHANG)
                            .Include(h => h.NHANVIEN)
                            .OrderByDescending(h => h.NGAYBAN);

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
            {
                hoadons = (IOrderedQueryable<HOADON>)hoadons.Where(h => h.TRANGTHAI == trangThai);
            }

            ViewBag.TrangThai = trangThai; // Giữ lại trạng thái đã chọn
            return View(hoadons.ToList());
        }

        //
        // GET: NhanVien/DonHang/Details/5
        //
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Lấy Hóa đơn và Chi tiết hóa đơn (kèm Sản phẩm)
            HOADON hoadon = db.HOADON
                              .Include(h => h.KHACHHANG)
                              .Include(h => h.NHANVIEN)
                              .Include(h => h.CHITIETHD.Select(ct => ct.SANPHAM))
                              .FirstOrDefault(h => h.MAHD == id);

            if (hoadon == null)
            {
                return HttpNotFound();
            }
            return View(hoadon);
        }

        //
        // POST: NhanVien/DonHang/DuyetDon/5
        // Dùng để duyệt các đơn hàng Online (đơn "Chờ duyệt")
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DuyetDon(int id)
        {
            var hoadon = db.HOADON.Find(id);
            if (hoadon == null)
            {
                return HttpNotFound();
            }

            // Gọi Stored Procedure để duyệt
            db.sp_DuyetDonHang(id);
            db.SaveChanges();

            TempData["Success"] = $"Đã duyệt thành công hóa đơn #{id}.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
