using ShopQLCHDT.Models;
using ShopQLCHDT.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace ShopQLCHDT.Controllers
{
    public class CartController : Controller
    {
        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();

        // Lấy giỏ hàng từ Session
        private List<CartItemViewModel> GetCart()
        {
            return Session["GioHang"] as List<CartItemViewModel> ?? new List<CartItemViewModel>();
        }

        // 1. Thêm vào giỏ
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            var sp = db.SANPHAM.Find(productId);
            var kho = db.KHO_SANPHAM.FirstOrDefault(k => k.MASP == productId);
            int tonKho = kho != null ? (int)kho.SOLUONGTON : 0;

            if (tonKho < quantity)
            {
                return Json(new { success = false, msg = "Hết hàng hoặc không đủ số lượng!" });
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSP == productId);

            if (item != null)
            {
                item.SoLuong += quantity;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    MaSP = sp.MASP,
                    TenSP = sp.TENSP,
                    HinhAnh = sp.HINHANH,
                    DonGia = (decimal)sp.GIABAN,
                    SoLuong = quantity,
                    SoLuongTon = tonKho
                });
            }

            Session["GioHang"] = cart;
            return RedirectToAction("Index");
        }

        // 2. Xem giỏ hàng
        public ActionResult Index()
        {
            return View(GetCart());
        }

        // 3. Thanh toán (Checkout) - QUAN TRỌNG
        [HttpPost]
        public ActionResult Checkout(string ghiChu)
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index");

            if (Session["MaKH"] == null) return RedirectToAction("Login", "Account");

            int maKH = (int)Session["MaKH"];
            // Giả sử đơn online chưa có nhân viên phụ trách, để NULL hoặc ID Admin mặc định
            int? maNV = 1; // ID Admin hoặc NV bán hàng Online

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {

                    var maHD = db.sp_LapHoaDon(maKH, maNV, 0, ghiChu).FirstOrDefault();

                    if (maHD != null)
                    {
                        int idHoaDon = (int)maHD;


                        foreach (var item in cart)
                        {
                            db.sp_LapChiTietHoaDon(idHoaDon, item.MaSP, item.SoLuong);
                        }


                        // Trigger TR_CHITIETHD_AFTER_INSERT
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    Session["GioHang"] = null; // Xóa giỏ
                    return View("Success");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ViewBag.Error = "Lỗi thanh toán: " + ex.Message;
                    return View("Index", cart);
                }
            }
        }
    }
}
