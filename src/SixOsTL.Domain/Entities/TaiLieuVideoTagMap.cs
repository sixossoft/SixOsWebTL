namespace SixOsTL.Domain.Entities
{
    public class TaiLieuVideoTagMap
    {
        public long IDVideo { get; set; }
        public long IDTag { get; set; }

        public TaiLieuVideo Video { get; set; } = null!;
        public TaiLieuVideoTag Tag { get; set; } = null!;
    }
}
