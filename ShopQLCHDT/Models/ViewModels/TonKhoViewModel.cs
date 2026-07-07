using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopQLCHDT.Models.ViewModels
{
    public class TonKhoViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuongTon { get; set; }
        public System.DateTime NgayCapNhatCuoi { get; set; }
    }
}