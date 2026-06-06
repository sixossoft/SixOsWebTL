namespace SixOsTL.Application.DTOs.TaiKhoan
{
    public record CreateTaiKhoanDto(
       string MaCSKCB,
       string HoTen,
       string TenTK,
       string MatKhau,
       string? SoDienThoai,
       string? Email,
       DateTime? NgayBatDau,
       DateTime? NgayKetThuc,
       IEnumerable<string> MaVaiTros   // ["USER"] hoặc ["ADMIN","USER"]
   );
}
