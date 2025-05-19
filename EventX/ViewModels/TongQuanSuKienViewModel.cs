using EventX.Models;

namespace EventX.ViewModels
{
    public class TongQuanSuKienViewModel
    {
        public Event? events;
        public List<Ticket> tickets;

        public decimal DoanhThu { get; set; }
        public decimal DoanhThuTruocKhuyenMai { get; set; }
        public decimal TienKhuyenMai { get; set; }

        public decimal TongPhi { get; set; }
        public decimal PhiDichVu { get; set; }
        public decimal PhiKhac { get; set; }

        public decimal ThucNhan { get; set; }

        public int TongSoVe { get; set; }
        public int SoVeDaBan { get; set; }
        public int SoVeConLai => TongSoVe - SoVeDaBan;

        public class DoanhThuNgay
        {
            public DateTime Ngay { get; set; }
            public decimal DoanhThu { get; set; }
            public int VeDaBan { get; set; }
        }
        public class DoanhThuTheoGioViewModel
        {
            public DateTime Gio { get; set; }  // chứa ngày giờ chính xác
            public decimal DoanhThu { get; set; }
            public int VeDaBan { get; set; }
        }
        public List<DoanhThuNgay> DoanhThuTheoNgay { get; set; }
        public List<DoanhThuTheoGioViewModel> DoanhThuTheoGio { get; set; }

        public class ChiTietVeViewModel
        {
            public string TenVe { get; set; }
            public decimal GiaVe { get; set; }
            public int TongSoLuong { get; set; }
            public int SoLuongDaBan { get; set; }
            public decimal DoanhThu { get; set; }
        }

        public List<ChiTietVeViewModel> ChiTietVeTheoLoai { get; set; }

    }
}
