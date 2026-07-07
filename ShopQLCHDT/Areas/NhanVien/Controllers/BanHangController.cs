using ShopQLCHDT.Models;
using ShopQLCHDT.Models.ViewModels;
using ShopQLCHDT.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ShopQLCHDT.Filters;


namespace ShopQLCHDT.Areas.NhanVien.Controllers
{

    public class BanHangController : Controller
    {
        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();
        private readonly OrderService _orderService = new OrderService();

        public ActionResult Index()
        {
            var model = new POSViewModel();

            // 1. Lấy danh sách sản phẩm để hiển thị (kèm tồn kho)
            model.DanhSachSanPham = db.SANPHAM
                .Where(s => s.TRANGTHAI == "Đang kinh doanh")
                .Select(s => new POSProductItem
                {
                    MaSP = s.MASP,
                    TenSP = s.TENSP,
                    GiaBan = (decimal)(s.GIABAN ?? 0),
                    HinhAnh = s.HINHANH,
                    // Lấy tồn kho mới nhất từ bảng KHO_SANPHAM
                    SoLuongTon = db.KHO_SANPHAM
                                   .Where(k => k.MASP == s.MASP)
                                   .OrderByDescending(k => k.NGAYCAPNHAT)
                                   .Select(k => k.SOLUONGTON)
                                   .FirstOrDefault() ?? 0
                }).ToList();


            return View(model);
        }
        [HttpGet] // Đánh dấu đây là Action GET
        public JsonResult SearchProducts(string keyword)
        {
            // Tìm kiếm tương đối (Contains) trong Tên SP và chỉ lấy 10 kết quả
            var products = db.SANPHAM
                .Where(s => s.TENSP.Contains(keyword) && s.TRANGTHAI == "Đang kinh doanh")
                .Select(s => new { // Trả về kiểu dữ liệu ẩn (anonymous type)
                    s.MASP,
                    s.TENSP,
                    s.GIABAN,
                    s.HINHANH,
                    // Lấy tồn kho
                    SoLuongTon = db.KHO_SANPHAM
                                   .Where(k => k.MASP == s.MASP)
                                   .OrderByDescending(k => k.NGAYCAPNHAT)
                                   .Select(k => k.SOLUONGTON)
                                   .FirstOrDefault() ?? 0
                })
                .Take(10)
                .ToList();

            // Cho phép trả về JSON cho request GET
            return Json(products, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult KiemTraKhachHang(string customerAccount, string ghiChu, List<OrderDetailItem> ChiTietDonHang)
        {
            // 1. Lưu giỏ hàng và ghi chú vào Session
            Session["POS_Cart"] = ChiTietDonHang;
            Session["POS_GhiChu"] = ghiChu;

            // 2. Kiểm tra tài khoản (ưu tiên SĐT)
            var khachHang = db.KHACHHANG.FirstOrDefault(kh => kh.DIENTHOAI == customerAccount);
            if (khachHang == null)
            {
                // Nếu không thấy SĐT, kiểm tra Tên đăng nhập
                var tk = db.TAIKHOAN.FirstOrDefault(t => t.TENDANGNHAP == customerAccount);
                if (tk != null)
                {
                    khachHang = db.KHACHHANG.FirstOrDefault(kh => kh.IDTK == tk.IDTK);
                }
            }

            // 3. Xử lý luồng
            if (khachHang != null)
            {
                // TÌM THẤY: Lưu MAKH và chuyển đến trang Xác nhận
                Session["POS_MAKH"] = khachHang.MAKH;
                return RedirectToAction("XacNhanHoaDon");
            }
            else
            {
                // KHÔNG TÌM THẤY: Lưu tài khoản mới và chuyển đến trang Tạo KH
                Session["POS_NewAccount"] = customerAccount; // Lưu SĐT hoặc Tên TK
                return RedirectToAction("TaoKhachHangMoi");
            }
        }

        //
        // BƯỚC 2.2: ACTION MỚI (GET + POST Tạo Khách Hàng)
        //
        public ActionResult TaoKhachHangMoi()
        {
            // Lấy SĐT/Tên TK từ session để điền sẵn
            ViewBag.NewAccount = Session["POS_NewAccount"] as string;
            return View(); // Trả về form tạo KH
        }

        [HttpPost]
        public ActionResult TaoKhachHangMoi(RegisterViewModel model)
        {
            try
            {
                // Gọi SP tạo khách hàng
                db.sp_ThemKhachHang(
                    model.TenDangNhap,
                    "123456", // Mật khẩu mặc định
                    model.TenKH,
                    model.GioiTinh,
                    model.DiaChi,
                    model.DienThoai,
                    "Khách hàng thân thiết");

                // Lấy MAKH vừa tạo
                var khMoi = db.KHACHHANG.FirstOrDefault(kh => kh.DIENTHOAI == model.DienThoai);

                // Lưu MAKH vào Session và chuyển đến trang Xác nhận
                Session["POS_MAKH"] = khMoi.MAKH;
                return RedirectToAction("XacNhanHoaDon");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View(model);
            }
        }

        //
        // BƯỚC 2.3: ACTION MỚI (Trang xác nhận cuối cùng)
        //
        public ActionResult XacNhanHoaDon()
        {
            // 1. LẤY TẤT CẢ DỮ LIỆU TỪ SESSION (Giống hệt LuuHoaDon)
            var maKH = Session["POS_MAKH"] != null ? (int)Session["POS_MAKH"] : 0;
            var cart = Session["POS_Cart"] as List<OrderDetailItem>;
            var ghiChu = Session["POS_GhiChu"] as string ?? ""; // Lấy ghi chú
            var tenNV = Session["TenHienThi"] as string ?? "Nhân viên"; // Lấy tên NV hiển thị

            // 2. Kiểm tra dữ liệu
            if (cart == null || cart.Count == 0 || maKH == 0)
            {
                TempData["Error"] = "Dữ liệu phiên làm việc đã hết hạn. Vui lòng thao tác lại.";
                return RedirectToAction("Index");
            }

            // 3. Truy vấn DB để lấy thông tin Khách & Cập nhật giá mới nhất (Bảo mật)
            var kh = db.KHACHHANG.Find(maKH);

            // Cập nhật lại giá và tên sản phẩm từ DB để tránh lỗi 0 đồng hoặc sai tên
            foreach (var item in cart)
            {
                var spDb = db.SANPHAM.Find(item.MaSP);
                if (spDb != null)
                {
                    item.DonGia = (decimal)spDb.GIABAN;
                    item.TenSP = spDb.TENSP;
                }
            }
            Session["POS_Cart"] = cart; // Cập nhật lại Session giỏ hàng sau khi lấy giá chuẩn

            // 4. Đẩy tất cả vào ViewModel
            var viewModel = new ConfirmOrderViewModel
            {
                KhachHang = kh,
                ChiTietDonHang = cart,
                GhiChu = ghiChu,      // Truyền ghi chú sang View
                TenNV = tenNV   // Truyền tên NV sang View
            };

            return View(viewModel);
        }
        //
        // BƯỚC 2.4: ACTION MỚI (Nút bấm cuối cùng để lưu vào DB)
        //
        [HttpPost]
        public ActionResult LuuHoaDon()
        {
            try
            {
                // Lấy tất cả dữ liệu từ Session
                int maKH = (int)Session["POS_MAKH"];
                int maNV = (int)Session["MANV"];
                string ghiChu = Session["POS_GhiChu"] as string;
                var cartItems = (Session["POS_Cart"] as List<OrderDetailItem>)
                    .Select(item => new CartItemViewModel
                    {
                        MaSP = item.MaSP,
                        SoLuong = item.SoLuong,
                        DonGia = item.DonGia
                    }).ToList();

                // 1. Gọi OrderService để tạo Hóa Đơn (bao gồm SP và Trigger)
                int newOrderId = _orderService.CreateOrder(maKH, maNV, ghiChu, cartItems);

                // 2. Duyệt đơn hàng ngay lập tức (vì là POS)
                db.sp_DuyetDonHang(newOrderId);
                db.SaveChanges();

                // 3. Xóa Session
                Session["POS_Cart"] = null;
                Session["POS_MAKH"] = null;
                Session["POS_GhiChu"] = null;

                TempData["Success"] = $"Tạo hóa đơn #{newOrderId} thành công!";
                return RedirectToAction("Index", "DonHang"); // Chuyển đến trang Lịch sử
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi lưu hóa đơn: " + ex.Message;
                return RedirectToAction("XacNhanHoaDon");
            }
        }
    }

    // Tạo ViewModel mới này trong thư mục /Models/ViewModels
    public class ConfirmOrderViewModel
    {
        public KHACHHANG KhachHang { get; set; }
        public List<OrderDetailItem> ChiTietDonHang { get; set; }
        public string GhiChu { get; internal set; }
        public string TenNV { get; internal set; }
    }
}
