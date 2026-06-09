namespace SixOsTL.Domain.Entities
{
    public class TaiKhoanChucNang
    {
        public long Id { get; set; }
        public long? IdCN { get; set; }
        public long? IdTK { get; set; }
        public bool Active { get; set; }

        public DmChucNang? ChucNang { get; set; }
        public TaiKhoanDaoTao? TaiKhoan { get; set; }
    }
}
