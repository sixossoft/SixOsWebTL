namespace SixOsTL.Domain.Entities
{
    public class TaiKhoanDaoTao
    {
        public long Id { get; set; }
        public string MaCSKCB { get; set; } = default!;
        public string? HoTen { get; set; }
        public string TenTK { get; set; } = default!;
        public string MatKhau { get; set; } = default!;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation
        public ThongTinDoanhNghiepDaoTao? CoSoKCB { get; set; }
        public ICollection<TaiKhoanVaiTro> TaiKhoanVaiTros { get; set; } = new List<TaiKhoanVaiTro>();
        public ICollection<TaiLieuHoiDap> HoiDaps { get; set; } = new List<TaiLieuHoiDap>();

        // kiểm tra tài khoản còn hiệu lực
        public bool ConHieuLuc()
        {
            var now = DateTime.Now;
            if (IsDeleted) return false;
            if (NgayBatDau.HasValue && now < NgayBatDau.Value) return false;
            if (NgayKetThuc.HasValue && now > NgayKetThuc.Value) return false;
            return true;
        }
    }
}
