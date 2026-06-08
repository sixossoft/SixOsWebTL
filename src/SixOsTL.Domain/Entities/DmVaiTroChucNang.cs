namespace SixOsTL.Domain.Entities
{
    public class DmVaiTroChucNang
    {
        public long IDVaiTro { get; set; }
        public long IDChucNang { get; set; }

        public DmVaiTro VaiTro { get; set; } = default!;
        public DmChucNang ChucNang { get; set; } = default!;
    }
}
