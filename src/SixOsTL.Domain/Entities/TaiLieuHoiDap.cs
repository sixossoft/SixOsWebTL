namespace SixOsTL.Domain.Entities
{
    public class TaiLieuHoiDap
    {
        public long Id { get; set; }
        public long IDChucNang { get; set; }
        public long IDTaiKhoan { get; set; }
        public string NoiDung { get; set; } = default!;
        public bool CongKhai { get; set; }
        public long? ParentHoiDapID { get; set; }
        public DateTime NgayTao { get; set; }
        public bool Active { get; set; }

        public DmChucNang ChucNang { get; set; } = default!;
        public TaiKhoanDaoTao TaiKhoan { get; set; } = default!;
        public TaiLieuHoiDap? ParentHoiDap { get; set; }
        public ICollection<TaiLieuHoiDap> TraLois { get; set; } = new List<TaiLieuHoiDap>();
    }
}
