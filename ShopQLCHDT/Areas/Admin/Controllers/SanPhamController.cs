using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web; // Cần cho HttpPostedFileBase
using System.Web.Mvc;
using ShopQLCHDT.Models;
using System.IO; // Cần cho xử lý Path (Đường dẫn file)

namespace ShopQLCHDT.Areas.Admin.Controllers
{
    // [Authorize(Roles = "Admin")] // (Bạn có thể thêm phân quyền sau)
    public class SanPhamController : Controller
    {
        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();

        // GET: Admin/SanPham
        // (Sử dụng LINQ để lấy dữ liệu, vì SP sp_XemDanhSachMatHang 
        // trả về kiểu dữ liệu mà View khó bind)
        public ActionResult Index()
        {
            var sanPhams = db.SANPHAM.Include(s => s.THUONGHIEU)
                                     .OrderByDescending(s => s.MASP)
                                     .ToList();
            return View(sanPhams);
        }

        // GET: Admin/SanPham/Create
        public ActionResult Create()
        {
            // Gửi danh sách Thương hiệu sang View
            ViewBag.MATH = new SelectList(db.THUONGHIEU, "MATH", "TENTH");
            return View();
        }

        // POST: Admin/SanPham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SANPHAM sanPham, HttpPostedFileBase HinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                // ----- XỬ LÝ UPLOAD HÌNH ẢNH -----
                if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
                {
                    // Lấy tên file
                    string fileName = Path.GetFileName(HinhAnhFile.FileName);
                    // Tạo đường dẫn lưu file (ví dụ: ~/Content/Images/tenfile.jpg)
                    string path = Path.Combine(Server.MapPath("~/Content/Images/"), fileName);

                    // Lưu file
                    HinhAnhFile.SaveAs(path);

                    // Gán tên file vào model
                    sanPham.HINHANH = fileName;
                }
                // ---------------------------------

                // Gọi Stored Procedure
                db.sp_ThemMatHang(
                    sanPham.TENSP,
                    sanPham.DVT,
                    sanPham.GIABAN,
                    sanPham.THONGTIN,
                    sanPham.HINHANH, // Tên file đã lưu
                    sanPham.MATH
                );

                TempData["Success"] = "Tạo sản phẩm thành công.";
                return RedirectToAction("Index");
            }

            // Nếu model state lỗi, tải lại Dropdownlist
            ViewBag.MATH = new SelectList(db.THUONGHIEU, "MATH", "TENTH", sanPham.MATH);
            return View(sanPham);
        }

        // GET: Admin/SanPham/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SANPHAM sanPham = db.SANPHAM.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            ViewBag.MATH = new SelectList(db.THUONGHIEU, "MATH", "TENTH", sanPham.MATH);
            return View(sanPham);
        }

        // POST: Admin/SanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SANPHAM sanPham, HttpPostedFileBase HinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                // ----- XỬ LÝ UPLOAD HÌNH ẢNH MỚI (NẾU CÓ) -----
                if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(HinhAnhFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/Images/"), fileName);
                    HinhAnhFile.SaveAs(path);
                    sanPham.HINHANH = fileName; // Gán tên file mới
                }
                // Nếu không có file mới, trường HINHANH (nhờ @Html.HiddenFor)
                // sẽ giữ nguyên giá trị cũ.
                // ---------------------------------------------

                // Gọi Stored Procedure
                db.sp_SuaMatHang(
                    sanPham.MASP,
                    sanPham.TENSP,
                    sanPham.DVT,
                    sanPham.GIABAN,
                    sanPham.THONGTIN,
                    sanPham.HINHANH,
                    sanPham.MATH
                );

                TempData["Success"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction("Index");
            }
            ViewBag.MATH = new SelectList(db.THUONGHIEU, "MATH", "TENTH", sanPham.MATH);
            return View(sanPham);
        }

        // GET: Admin/SanPham/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SANPHAM sanPham = db.SANPHAM.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            return View(sanPham);
        }

        // POST: Admin/SanPham/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Gọi Stored Procedure
            db.sp_XoaMatHang(id); // SP này chỉ cập nhật trạng thái

            TempData["Success"] = "Đã ngừng kinh doanh sản phẩm.";
            return RedirectToAction("Index");
        }
        public ActionResult Details(int id)
        {
            var sp = db.SANPHAM.Find(id);
            if (sp == null) return HttpNotFound();


            var tonKho = db.KHO_SANPHAM.FirstOrDefault(k => k.MASP == id);
            ViewBag.SoLuongTon = tonKho != null ? tonKho.SOLUONGTON : 0;

            return View(sp);
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