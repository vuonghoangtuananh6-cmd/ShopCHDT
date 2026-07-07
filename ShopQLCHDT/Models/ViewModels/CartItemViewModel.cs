using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShopQLCHDT.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int MaSP { get; set; }

        public string TenSP { get; set; }

        public string HinhAnh { get; set; }

        // Dùng decimal cho tiền tệ như trong SQL (GIABAN DECIMAL(18, 2))
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal DonGia { get; set; }

        [Range(1, 100, ErrorMessage = "Số lượng tối thiểu là 1")]
        public int SoLuong { get; set; }

        // Lấy từ bảng KHO_SANPHAM để kiểm tra max số lượng được mua
        public int SoLuongTon { get; set; }

        // Thuộc tính tính toán (Read-only), không cần lưu DB
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal ThanhTien
        {
            get { return SoLuong * DonGia; }
        }
    }
}