namespace SixOsTL.Domain.Entities
{
    public class TaiLieuFile
    {
        public long Id { get; set; }
        public string? STT { get; set; }
        public long IDChucNang { get; set; }
        public string? TenFile { get; set; }
        public string? Keyword { get; set; }
        public string? DuongDanFile { get; set; }
        public bool Active { get; set; }

        public DmChucNang ChucNang { get; set; } = default!;
    }
}
