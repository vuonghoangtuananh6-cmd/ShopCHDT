using ShopQLCHDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ShopQLCHDT.Models.ViewModels;

namespace ShopQLCHDT.Controllers
{
    public class AccountController : Controller
    {
        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();

        // GET: Đăng ký
        public ActionResult Register()
        {
            return View();
        }

        // POST: Đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                try
                {
                    db.sp_ThemKhachHang(
                        model.TenDangNhap,
                        model.MatKhau,
                        model.TenKH,
                        model.GioiTinh,
                        model.DiaChi,
                        model.DienThoai,
                        "Khách hàng thân thiết" // Mặc định loại KH
                    );

                    TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                catch
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc SĐT đã tồn tại.");
                }
            }
            return View(model);
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // 1. Kiểm tra tài khoản
            var acc = db.TAIKHOAN.FirstOrDefault(t => t.TENDANGNHAP == username && t.MATKHAU == password);

            if (acc != null)
            {
                // Kiểm tra trạng thái
                if (acc.TRANGTHAI != "Hoạt động")
                {
                    ViewBag.Error = "Tài khoản đã bị khóa!";
                    return View("Login");
                }

                // 2. Lưu thông tin cơ bản vào Session
                Session["UserID"] = acc.IDTK;
                Session["Username"] = acc.TENDANGNHAP;
                Session["Role"] = acc.LOAITK; // "Khách hàng", "Nhân viên", "Admin"

                // 3. Xử lý riêng cho từng loại tài khoản
                if (acc.LOAITK == "Khách hàng")
                {
                    var kh = db.KHACHHANG.FirstOrDefault(k => k.IDTK == acc.IDTK);
                    if (kh != null)
                    {
                        Session["MaKH"] = kh.MAKH;
                        Session["TenHienThi"] = kh.TENKH; // Hiển thị tên thật thay vì user
                    }
                }
                else if (acc.LOAITK == "Nhân viên")
                {
                    var nv = db.NHANVIEN.FirstOrDefault(n => n.IDTK == acc.IDTK);
                    if (nv != null)
                    {
                        Session["MANV"] = nv.MANV;
                        Session["TenHienThi"] = nv.TENNV;

                        // --- BẮT ĐẦU LOGIC PHÂN QUYỀN ---

                        // Tìm dòng phân quyền của nhân viên này
                        var quyen = db.PHANQUYEN.FirstOrDefault(p => p.MANV == nv.MANV);

                        if (quyen != null)
                        {
                            // Lưu các quyền quan trọng vào Session (ép kiểu về bool)
                            // Lưu ý: Tên Session này phải khớp với code trong _Layout

                            // Quyền Bán Hàng (liên quan đến POS, Đơn hàng)
                            Session["Quyen_BanHang"] = (quyen.QL_DONHANG == true);

                            // Quyền Nhập Hàng (liên quan đến Tạo phiếu nhập)
                            Session["Quyen_NhapHang"] = (quyen.QL_NHAPHANG == true);

                            // Quyền Kho (liên quan đến Tồn kho, Nhà cung cấp)
                            Session["Quyen_TonKho"] = (quyen.QL_BPKHO == true);

                            // Quyền Thống Kê
                            Session["Quyen_ThongKe"] = (quyen.THONGKE == true);
                        }
                        else
                        {
                            // Nếu chưa được cấp quyền nào -> Set tất cả là false
                            Session["Quyen_BanHang"] = false;
                            Session["Quyen_NhapHang"] = false;
                            Session["Quyen_TonKho"] = false;
                            Session["Quyen_ThongKe"] = false;
                        }
                        // --- KẾT THÚC LOGIC PHÂN QUYỀN ---
                    }
                }
                else if (acc.LOAITK == "Admin")
                {
                    // Admin thì có toàn quyền (Layout sẽ check Role == "Admin")
                    Session["TenHienThi"] = "Quản Trị Viên";
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
            return View("Login");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
