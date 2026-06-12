namespace SixOsTL.Domain.Entities
{
    public class DmChucNang
    {
        public long Id { get; set; }
        public long IDSanPham { get; set; }
        public long? IDMucDoUuTien { get; set; }
        public string? ChucNang { get; set; }
        public string? DuongDanFile { get; set; }
        public bool Active { get; set; }
        public int Stt { get; set; }
        public DmSanPham SanPham { get; set; } = default!;
        public DmMucDoUuTien? MucDoUuTien { get; set; }
        public ICollection<TaiLieuVideo> Videos { get; set; } = new List<TaiLieuVideo>();
        public ICollection<TaiLieuFile> Files { get; set; } = new List<TaiLieuFile>();
        public ICollection<TaiLieuHoiDap> HoiDaps { get; set; } = new List<TaiLieuHoiDap>();
        public ICollection<TaiKhoanChucNang> TaiKhoanChucNangs { get; set; } = new List<TaiKhoanChucNang>();
    }
}
