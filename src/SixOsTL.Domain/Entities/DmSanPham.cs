namespace SixOsTL.Domain.Entities
{
    public class DmSanPham
    {
        public long Id { get; set; }
        public string TenSP { get; set; } = default!;
        public bool Active { get; set; }

        public ICollection<DmChucNang> ChucNangs { get; set; } = new List<DmChucNang>();
    }
}
