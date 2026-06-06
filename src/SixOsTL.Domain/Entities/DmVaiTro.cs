namespace SixOsTL.Domain.Entities
{
    public class DmVaiTro
    {
        public long Id { get; set; }
        public string MaVaiTro { get; set; } = default!;
        public string TenVaiTro { get; set; } = default!;
        public string? MoTa { get; set; }
        public bool Active { get; set; }
        public ICollection<TaiKhoanVaiTro> TaiKhoanVaiTros { get; set; } = new List<TaiKhoanVaiTro>();
    }
}
