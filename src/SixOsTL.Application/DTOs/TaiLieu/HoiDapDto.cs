namespace SixOsTL.Application.DTOs.TaiLieu
{
    public record HoiDapDto(
        long Id,
        long IDChucNang,
        long IDTaiKhoan,
        string? TenNguoiHoi,
        string NoiDung,
        bool CongKhai,
        bool Active,
        long? ParentHoiDapID,
        DateTime NgayTao,
        IEnumerable<HoiDapDto> TraLois,
        IEnumerable<HoiDapHinhAnhDto> HinhAnhs
    );
}
