using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace ShopQLCHDT.Models.ViewModels
{
    public class NhanVienCreateViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        // Thuộc tính cho NHANVIEN
        [Required(ErrorMessage = "Tên nhân viên là bắt buộc")]
        public string TenNV { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public string GioiTinh { get; set; }

        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Lương là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Lương phải là số dương")]
        public decimal Luong { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string DienThoai { get; set; }
    }
}