using ShopQLCHDT.Filters;
using ShopQLCHDT.Models;
using ShopQLCHDT.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopQLCHDT.Areas.Kho.Controllers
{

    public class NhapHangController : Controller
    {
        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();

        // GET: Kho/NhapHang (Hiển thị các phiếu nhập cũ)
        public ActionResult Index()
        {
            var phieuNhaps = db.PHIEUNHAP
                               .Include("NHACUNGCAP")
                               .OrderByDescending(p => p.NGAYNHAP)
                               .ToList();
            return View(phieuNhaps);
        }

        // GET: Kho/NhapHang/Create
        // Giao diện tạo phiếu nhập (Master-Detail form)
        public ActionResult Create()
        {
            var model = new PhieuNhapViewModel();

            // Chuẩn bị data cho các Dropdownlist
            ViewBag.NhaCungCapList = new SelectList(db.NHACUNGCAP, "MANCC", "TENNCC");
            // Dùng 1 Partial View để load danh sách sản phẩm cho Javascript
            ViewBag.SanPhamList = db.SANPHAM
                .Select(s => new { s.MASP, s.TENSP, s.GIABAN })
                .ToList();

            return View(model);
        }

        // POST: Kho/NhapHang/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PhieuNhapViewModel model, int MaNQL) // MaNQL lấy từ Session
        {
            if (model.DanhSachHangNhap == null || model.DanhSachHangNhap.Count == 0)
            {
                ModelState.AddModelError("", "Phiếu nhập phải có ít nhất 1 sản phẩm.");
            }

            if (ModelState.IsValid)
            {
                // Bắt đầu Transaction để đảm bảo an toàn
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Bước 1: Gọi SP tạo Phiếu Nhập (Header)
                        // [cite: 70]
                        var result = db.sp_LapPhieuNhap(
                            model.MaNCC,
                            MaNQL, // Lấy MaNQL (Mã người quản lý kho) từ Session
                            model.GhiChu
                        ).FirstOrDefault(); // [cite: 71]

                        if (result == null) throw new Exception("Không thể tạo phiếu nhập.");

                        int newPhieuNhapID = (int)result;

                        // Bước 2: Duyệt list và gọi SP tạo Chi Tiết Phiếu Nhập
                        foreach (var item in model.DanhSachHangNhap)
                        {
                             db.sp_LapChiTietPhieuNhap( 
                                newPhieuNhapID,
                                item.MaSP,
                                item.SoLuong,
                                item.GiaNhap
                            );

                            // Trigger TR_CHITIETPN_AFTER_INSERT  
                            // sẽ tự động chạy tại đây để CỘNG VÀO KHO_SANPHAM
                        }

                        transaction.Commit(); // Hoàn tất giao dịch
                        TempData["Success"] = "Tạo phiếu nhập thành công!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Hoàn tác nếu có lỗi
                        ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                    }
                }
            }

            // Nếu lỗi, load lại data cho View
            ViewBag.NhaCungCapList = new SelectList(db.NHACUNGCAP, "MANCC", "TENNCC", model.MaNCC);
            ViewBag.SanPhamList = db.SANPHAM.Select(s => new { s.MASP, s.TENSP, s.GIABAN }).ToList();
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
