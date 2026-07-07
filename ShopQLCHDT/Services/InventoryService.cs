using ShopQLCHDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
namespace ShopQLCHDT.Services
{
    public class InventoryService
    {
        private readonly QLY_SHOPDTDD1Entities db;

        public InventoryService()
        {
            db = new QLY_SHOPDTDD1Entities();
        }

        // 1. Lấy số lượng tồn hiện tại của một sản phẩm
       // Logic: Vì bảng KHO_SANPHAM lưu lịch sử theo ngày (PK: MASP, NGAYCAPNHAT) [cite: 15]
        // Nên ta cần lấy dòng có NGAYCAPNHAT mới nhất của sản phẩm đó.
        public int GetStockQuantity(int maSP)
        {
            var stockItem = db.KHO_SANPHAM
                              .Where(k => k.MASP == maSP)
                              .OrderByDescending(k => k.NGAYCAPNHAT)
                              .FirstOrDefault();

            return stockItem != null ? (int)stockItem.SOLUONGTON : 0;
        }

        // 2. Kiểm tra xem có đủ hàng để thêm vào giỏ không
        public bool CheckStockAvailability(int maSP, int quantityRequested)
        {
            int currentStock = GetStockQuantity(maSP);
            return currentStock >= quantityRequested;
        }

        // 3. (Dành cho Thủ kho) Lấy danh sách các món sắp hết hàng (dưới mức cảnh báo)
        public IQueryable<object> GetLowStockAlert(int threshold = 5)
        {
            // Lấy danh sách mã SP và số lượng tồn mới nhất
            var latestStocks = db.KHO_SANPHAM
                                 .GroupBy(k => k.MASP)
                                 .Select(g => g.OrderByDescending(k => k.NGAYCAPNHAT).FirstOrDefault());

            var query = from k in latestStocks
                        join p in db.SANPHAM on k.MASP equals p.MASP
                        where k.SOLUONGTON <= threshold
                        select new
                        {
                            p.MASP,
                            p.TENSP,
                            Stock = k.SOLUONGTON,
                            p.HINHANH
                        };

            return query;
        }
    }
}