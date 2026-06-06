namespace SixOsTL.Domain.Entities
{
    public class TaiKhoanVaiTro
    {
        public long IDTaiKhoan { get; set; }
        public long IDVaiTro { get; set; }

        public TaiKhoanDaoTao TaiKhoan { get; set; } = default!;
        public DmVaiTro VaiTro { get; set; } = default!;
    }
}
