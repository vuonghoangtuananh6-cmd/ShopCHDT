using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopQLCHDT.Models.ViewModels
{
    public class POSViewModel
    {
        // Danh sách sản phẩm để nhân viên tìm kiếm (Select list hoặc Table)
        public List<POSProductItem> DanhSachSanPham { get; set; }

        // Danh sách khách hàng để chọn nhanh
        public List<POSCustomerItem> DanhSachKhachHang { get; set; }
    }

    // Class phụ: Chứa thông tin gọn nhẹ của SP để hiển thị trên POS
    public class POSProductItem
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string Barcode { get; set; } // Nếu có mã vạch
        public decimal GiaBan { get; set; }
        public int SoLuongTon { get; set; } // Quan trọng: Để disable nút thêm nếu hết hàng
        public string HinhAnh { get; set; }
    }

    // Class phụ: Chứa thông tin khách hàng
    public class POSCustomerItem
    {
        public int MaKH { get; set; }
        public string TenKH { get; set; }
        public string SDT { get; set; }
    }

    // 2. Dùng để nhận dữ liệu khi bấm "Thanh Toán" (POST /BanHang/ThanhToan)
    public class OrderSubmissionModel
    {
        public int MaKH { get; set; } // ID khách hàng (nếu khách lẻ thì set ID mặc định)
        public string GhiChu { get; set; }
        public decimal GiamGia { get; set; }
        public List<OrderDetailItem> ChiTietDonHang { get; set; }
    }

    public class OrderDetailItem
    {
        public int MaSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; } // Giá tại thời điểm bán (đề phòng giá gốc đổi)
        public object TenSP { get; internal set; }
    }
}