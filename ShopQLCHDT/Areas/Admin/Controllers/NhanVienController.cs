using ShopQLCHDT.Models;
using ShopQLCHDT.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
namespace ShopQLCHDT.Areas.Admin.Controllers
{
    public class NhanVienController : Controller
    {
        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();

        // GET: Admin/NhanVien
        // Dùng SP sp_XemDanhSachNhanVien [cite: 89]
        public ActionResult Index()
        {
            // Gọi SP_XemDanhSachNhanVien
            // Tuy nhiên, để dễ binding trong MVC, 
            // chúng ta nên dùng EF Query trực tiếp sẽ tốt hơn
            var nhanViens = db.NHANVIEN.Include(n => n.TAIKHOAN)
                                        .Where(n => n.TAIKHOAN.LOAITK == "Nhân viên")
                                        .ToList();
            return View(nhanViens);
        }

        // GET: Admin/NhanVien/Create
        public ActionResult Create()
        {
            return View(new NhanVienCreateViewModel());
        }

        // POST: Admin/NhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NhanVienCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Gọi SP sp_ThemNhanVien 
                    // SP này đã bao gồm Transaction  và tự tạo PHANQUYEN 
                    db.sp_ThemNhanVien(
                        model.TenDangNhap,
                        model.MatKhau,
                        model.TenNV,
                        model.GioiTinh,
                        model.DiaChi,
                        model.Luong,
                        model.DienThoai
                    );

                    var nvMoi = db.NHANVIEN.FirstOrDefault(n => n.TAIKHOAN.TENDANGNHAP == model.TenDangNhap);

                    if (nvMoi != null)
                    {
                        TempData["Success"] = "Tạo nhân viên thành công. Vui lòng phân quyền ngay.";

                        // 3. Chuyển hướng sang trang Phân Quyền với ID vừa tìm được
                        // Lưu ý: Thêm area = "Admin" để đảm bảo đường dẫn chính xác
                        return RedirectToAction("Edit", "PhanQuyen", new { id = nvMoi.MANV, area = "Admin" });
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }
            return View(model);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            NHANVIEN nv = db.NHANVIEN.Find(id);
            if (nv == null) return HttpNotFound();
            return View(nv);
        }

        // POST: Admin/NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NHANVIEN model)
        {
            if (ModelState.IsValid)
            {
                // Gọi SP sp_SuaNhanVien [cite: 96]
                db.sp_SuaNhanVien(
                    model.MANV,
                    model.TENNV,
                    model.GIOITINH,
                    model.DIACHI,
                    (decimal)model.LUONG,
                    model.DIENTHOAI
                );
                TempData["Success"] = "Cập nhật thông tin thành công.";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Admin/NhanVien/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            NHANVIEN nv = db.NHANVIEN.Include(n => n.TAIKHOAN).FirstOrDefault(n => n.MANV == id);
            if (nv == null) return HttpNotFound();
            return View(nv);
        }

        // POST: Admin/NhanVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Gọi SP sp_XoaNhanVien [cite: 97]
            // SP này chỉ vô hiệu hóa tài khoản (Soft Delete)
            db.sp_XoaNhanVien(id);
            TempData["Success"] = "Đã vô hiệu hóa tài khoản nhân viên.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

