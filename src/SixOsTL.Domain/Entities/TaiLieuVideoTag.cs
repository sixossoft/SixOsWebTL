namespace SixOsTL.Domain.Entities
{
    public class TaiLieuVideoTag
    {
        public long Id { get; set; }
        public string TenTag { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public ICollection<TaiLieuVideoTagMap> VideoTagMaps { get; set; } = new List<TaiLieuVideoTagMap>();
        public ICollection<TaiLieuVideoLienQuan> LienQuans { get; set; } = new List<TaiLieuVideoLienQuan>();
    }
}
