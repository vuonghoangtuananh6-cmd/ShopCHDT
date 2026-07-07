using ShopQLCHDT.Models;
using ShopQLCHDT.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
namespace ShopQLCHDT.Services
{
    public class OrderService
    {
        private readonly QLY_SHOPDTDD1Entities db;
        private readonly InventoryService _inventoryService;

        public OrderService()
        {
            db = new QLY_SHOPDTDD1Entities();
            _inventoryService = new InventoryService();
        }

        // Hàm tạo đơn hàng hoàn chỉnh (Dùng cho cả Web và POS)
        public int CreateOrder(int maKH, int maNV, string ghiChu, List<CartItemViewModel> cartItems)
        {
            // 1. Validate lần cuối trước khi ghi DB
            foreach (var item in cartItems)
            {
                if (!_inventoryService.CheckStockAvailability(item.MaSP, item.SoLuong))
                {
                    throw new Exception($"Sản phẩm {item.TenSP} không đủ số lượng tồn kho!");
                }
            }

            // 2. Bắt đầu Transaction
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // A. Tạo Hóa Đơn (Header)
                    // Gọi Stored Procedure: sp_LapHoaDon 
                    // Lưu ý: Cần Add Function Import trong file .edmx để có hàm này
                    var result = db.sp_LapHoaDon(maKH, maNV, 0, ghiChu).FirstOrDefault();

                    if (result == null) throw new Exception("Không thể tạo hóa đơn.");

                    int newOrderId = Convert.ToInt32(result); // Lấy MAHD vừa tạo

                    // B. Tạo Chi Tiết Hóa Đơn (Details)
                    foreach (var item in cartItems)
                    {
                        // Gọi Stored Procedure: sp_LapChiTietHoaDon 
                        db.sp_LapChiTietHoaDon(newOrderId, item.MaSP, item.SoLuong);

                        // QUAN TRỌNG: Tại bước này, Trigger [cite: 105] trong SQL 
                        // sẽ tự động chạy để TRỪ KHO. C# không cần làm gì thêm.
                    }

                    db.SaveChanges();
                    transaction.Commit(); // Chốt đơn

                    return newOrderId;
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Có lỗi thì hoàn tác tất cả
                    throw ex; // Ném lỗi ra Controller để hiển thị
                }
            }
        }
    }
}