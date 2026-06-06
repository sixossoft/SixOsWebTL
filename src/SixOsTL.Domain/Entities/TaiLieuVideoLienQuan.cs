namespace SixOsTL.Domain.Entities
{
    public class TaiLieuVideoLienQuan
    {
        public long Id { get; set; }
        public long IDVideo { get; set; }           // video gốc
        public long IDVideoLienQuan { get; set; }   // video liên quan
        public long? IDTag { get; set; }            // null = manual
        public int STT { get; set; }
        public bool Active { get; set; } = true;

        public TaiLieuVideo Video { get; set; } = null!;
        public TaiLieuVideo VideoLienQuan { get; set; } = null!;
        public TaiLieuVideoTag? Tag { get; set; }
    }
}
