using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShopQLCHDT.Models.ViewModels
{
    public class PhieuNhapViewModel
    {
        // --- Thông tin phiếu nhập (Master) ---
        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
        public int MaNCC { get; set; }

        public string TenNCC { get; set; } // Để hiển thị lại nếu validate lỗi

        public string GhiChu { get; set; }

        // --- Chi tiết các món nhập (Details) ---
        // Danh sách này sẽ được bind từ giao diện nhập nhiều dòng
        public List<ChiTietNhapItem> DanhSachHangNhap { get; set; }

        public PhieuNhapViewModel()
        {
            DanhSachHangNhap = new List<ChiTietNhapItem>();
        }
    }

    public class ChiTietNhapItem
    {
        public int MaSP { get; set; }

        // Tên SP để hiển thị cho thủ kho check lại
        public string TenSP { get; set; }

        [Required]
        [Range(1, 10000, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá nhập không hợp lệ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal GiaNhap { get; set; } // Lưu ý: Đây là GIANHAP trong bảng CHITIETPN

        public decimal ThanhTien => SoLuong * GiaNhap;
    }
}