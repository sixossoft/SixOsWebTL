namespace SixOsTL.Domain.Entities
{
    public class DmMucDoUuTien
    {
        public long Id { get; set; }
        public string? STT { get; set; }
        public string MucDo { get; set; } = default!;

        public ICollection<DmChucNang> ChucNangs { get; set; } = new List<DmChucNang>();
    }
}
