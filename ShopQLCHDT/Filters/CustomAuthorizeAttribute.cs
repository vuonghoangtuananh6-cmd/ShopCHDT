using ShopQLCHDT.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopQLCHDT.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        // 1. Tạo một biến để lưu mã quyền mà chúng ta muốn kiểm tra
        // Ví dụ: "QL_DONHANG", "QL_NHAPHANG"...
        public string PermissionCode { get; set; }

        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();

        // 2. Ghi đè (override) phương thức kiểm tra quyền cốt lõi
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            // 3. Lấy MANV (Mã Nhân Viên) từ Session
            // (Điều này BẮT BUỘC bạn phải lưu MANV vào Session khi đăng nhập)
            var manv_session = httpContext.Session["MANV"];
            if (manv_session == null)
            {
                return false; // Không có Session MANV
            }

            int manv = (int)manv_session;

            // 4. Truy vấn bảng PHANQUYEN
            var permissions = db.PHANQUYEN.FirstOrDefault(p => p.MANV == manv);
            if (permissions == null)
            {
                return false; // Nhân viên này không có dòng nào trong bảng PHANQUYEN
            }

            // 5. Kiểm tra quyền cụ thể dựa trên PermissionCode
            switch (PermissionCode)
            {
                case "QL_DONHANG":
                    return permissions.QL_DONHANG ?? false;
                case "QL_SANPHAM":
                    return permissions.QL_SANPHAM ?? false;
                case "QL_KHACHHANG":
                    return permissions.QL_KHACHHANG ?? false;
                case "QL_NHANVIEN":
                    return permissions.QL_NHANVIEN ?? false;
                case "QL_NHACC":
                    return permissions.QL_NHACC ?? false;
                case "QL_NHAPHANG":
                    return permissions.QL_NHAPHANG ?? false;
                case "QL_BPKHO":
                    return permissions.QL_BPKHO ?? false;
                case "THONGKE":
                    return permissions.THONGKE ?? false;

                // (Thêm các trường hợp khác nếu bạn mở rộng bảng PHANQUYEN)

                default:
                    return false; // Mã quyền không xác định
            }
        }

        // 6. Nếu kiểm tra thất bại (return false), chuyển hướng về trang Đăng nhập
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary(
                    new { controller = "Account", action = "Login", area = "" }
                )
            );
        }
    }
}