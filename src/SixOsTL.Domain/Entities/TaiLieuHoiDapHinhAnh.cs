namespace SixOsTL.Domain.Entities
{
    public class TaiLieuHoiDapHinhAnh
    {
        public long Id { get; set; }
        public long IdTLHD { get; set; }
        public string DuongDanFileAnh { get; set; } = default!;

        // Navigation property
        public TaiLieuHoiDap HoiDap { get; set; } = default!;
    }
}
