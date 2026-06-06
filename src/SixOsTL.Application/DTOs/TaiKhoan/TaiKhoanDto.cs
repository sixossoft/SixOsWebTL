namespace SixOsTL.Application.DTOs.TaiKhoan
{
    public record TaiKhoanDto(
        long Id,
        string TenTK,
        string? HoTen,
        string MaCSKCB,
        string? SoDienThoai,
        string? Email,
        DateTime? NgayBatDau,
        DateTime? NgayKetThuc,
        IEnumerable<string> Roles
    );
}
