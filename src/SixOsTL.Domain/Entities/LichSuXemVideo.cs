namespace SixOsTL.Domain.Entities
{
    public class LichSuXemVideo
    {
        public long Id { get; set; }
        public long IDVideo { get; set; }
        public long IDTaiKhoanDT { get; set; }
        public int? Phut { get; set; }
        public int? Giay { get; set; }

        public TaiLieuVideo Video { get; set; } = default!;
        public TaiKhoanDaoTao TaiKhoanDaoTao { get; set; } = default!;
    }
}