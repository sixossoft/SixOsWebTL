namespace SixOsTL.Domain.Entities
{
    public class ThongTinDoanhNghiepDaoTao
    {
        public long Id { get; set; }
        public string MaCSKCB { get; set; } = default!;
        public string? TenCSKCBVietTat { get; set; }
        public string? TenCSKCB { get; set; }
        public bool GioiHan { get; set; }
        public bool Active { get; set; }
        public ICollection<TaiKhoanDaoTao> TaiKhoans { get; set; } = new List<TaiKhoanDaoTao>();
    }
}
