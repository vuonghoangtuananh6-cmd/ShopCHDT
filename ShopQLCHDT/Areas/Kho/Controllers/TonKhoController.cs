using ShopQLCHDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShopQLCHDT.Models.ViewModels;

namespace ShopQLCHDT.Areas.Kho.Controllers
{
    public class TonKhoController : Controller
    {
        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();

        // GET: Kho/TonKho
        // Hiển thị số lượng tồn kho mới nhất của tất cả sản phẩm
        public ActionResult Index()
        {
            // Do KHO_SANPHAM lưu lịch sử, ta cần Group by MASP
            // và lấy dòng có NGAYCAPNHAT mới nhất
            var latestStocks = db.KHO_SANPHAM
                                 .GroupBy(k => k.MASP)
                                 .Select(g => g.OrderByDescending(k => k.NGAYCAPNHAT).FirstOrDefault());

            // Join với bảng Sản phẩm để hiển thị thông tin
            var tonKhoList = from k in latestStocks
                             join s in db.SANPHAM on k.MASP equals s.MASP
                             where k.SOLUONGTON > 0 // Chỉ hiện các món còn tồn
                             select new TonKhoViewModel // Cần tạo ViewModel này
                             {
                                 MaSP = s.MASP,
                                 TenSP = s.TENSP,
                                 HinhAnh = s.HINHANH,
                                 SoLuongTon = (int)(k.SOLUONGTON ?? 0),
                                 NgayCapNhatCuoi = (System.DateTime)(k.NGAYCAPNHAT)
                             };

            return View(tonKhoList.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }


}

