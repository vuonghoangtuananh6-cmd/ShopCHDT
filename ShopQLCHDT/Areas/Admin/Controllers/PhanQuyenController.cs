using ShopQLCHDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
namespace ShopQLCHDT.Areas.Admin.Controllers
{
    public class PhanQuyenController : Controller
    {
        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();

        // GET: Admin/PhanQuyen
        // Hiển thị danh sách nhân viên để chọn
        public ActionResult Index()
        {
            var nhanViens = db.NHANVIEN
                             .Include(n => n.TAIKHOAN)
                             .Where(n => n.TAIKHOAN.LOAITK == "Nhân viên" && n.TAIKHOAN.TRANGTHAI == "Hoạt động")
                             .ToList();
            return View(nhanViens);
        }

        // GET: Admin/PhanQuyen/Edit/5
        // Hiển thị form checkbox quyền cho nhân viên (MANV = id)
        public ActionResult Edit(int id)
        {
            // Lấy thông tin phân quyền của nhân viên
            PHANQUYEN quyen = db.PHANQUYEN.FirstOrDefault(p => p.MANV == id);

            // Nếu nhân viên này chưa có dòng nào trong bảng PHANQUYEN
            if (quyen == null)
            {
                // Tạo một đối tượng mới với MANV
                quyen = new PHANQUYEN() { MANV = id };
            }

            // Lấy tên nhân viên để hiển thị
            ViewBag.TenNV = db.NHANVIEN.Find(id)?.TENNV;

            return View(quyen);
        }

        // POST: Admin/PhanQuyen/Edit
        // Lưu các checkbox
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PHANQUYEN model)
        {
            if (ModelState.IsValid)
            {
                // Gọi SP sp_CapNhatQuyen [cite: 98]
                // SP này dùng MERGE nên tự biết khi nào UPDATE, khi nào INSERT [cite: 99, 100, 101]
                db.sp_CapNhatQuyen(
                    model.MANV,
                    model.QL_DONHANG ?? false,
                    model.QL_SANPHAM ?? false,
                    model.QL_KHACHHANG ?? false,
                    model.QL_NHANVIEN ?? false,
                    model.QL_NHACC ?? false,
                    model.QL_NHAPHANG ?? false,
                    model.QL_BPKHO ?? false,
                    model.THONGKE ?? false
                );

                TempData["Success"] = "Cập nhật quyền hạn thành công.";
                return RedirectToAction("Index");
            }

            ViewBag.TenNV = db.NHANVIEN.Find(model.MANV)?.TENNV;
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
    
}
