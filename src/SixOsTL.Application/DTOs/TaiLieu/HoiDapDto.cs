namespace SixOsTL.Application.DTOs.TaiLieu
{
    public record HoiDapDto(
        long Id,
        long IDChucNang,
        long IDTaiKhoan,
        string? TenNguoiHoi,
        string NoiDung,
        bool CongKhai,
        long? ParentHoiDapID,
        DateTime NgayTao,
        IEnumerable<string>? DanhSachAnhs,
        IEnumerable<HoiDapDto> TraLois
    );
}
