using ShopQLCHDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
namespace ShopQLCHDT.Services
{
    public class AuthService
    {
        private readonly QLY_SHOPDTDD1Entities db;

        public AuthService()
        {
            db = new QLY_SHOPDTDD1Entities();
        }

        // 1. Xử lý đăng nhập
        public TAIKHOAN Login(string username, string password)
        {
            // Tìm tài khoản trùng khớp và đang hoạt động [cite: 2]
            return db.TAIKHOAN.FirstOrDefault(t =>
                t.TENDANGNHAP == username &&
                t.MATKHAU == password &&
                t.TRANGTHAI == "Hoạt động");
        }

        // 2. Lấy thông tin Nhân viên từ Tài khoản
        public NHANVIEN GetEmployeeInfo(int idTK)
        {
            return db.NHANVIEN.FirstOrDefault(nv => nv.IDTK == idTK);
        }

        // 3. Lấy thông tin Khách hàng từ Tài khoản
        public KHACHHANG GetCustomerInfo(int idTK)
        {
            return db.KHACHHANG.FirstOrDefault(kh => kh.IDTK == idTK);
        }

        // 4. Kiểm tra quyền hạn cụ thể (Dành cho Admin/Phân quyền)
        // Ví dụ: Kiểm tra xem nhân viên này có được phép Nhập Hàng không?
        public bool HasPermission(int maNV, string permissionCode)
        {
            var quyen = db.PHANQUYEN.FirstOrDefault(p => p.MANV == maNV);
            if (quyen == null) return false;

            switch (permissionCode)
            {
                case "QL_NHAPHANG": return quyen.QL_NHAPHANG ?? false;
                case "QL_DONHANG": return quyen.QL_DONHANG ?? false;
            case "QL_BPKHO": return quyen.QL_BPKHO ?? false;
            case "THONGKE": return quyen.THONGKE ?? false;
            default: return false;
            }
        }
    }
}