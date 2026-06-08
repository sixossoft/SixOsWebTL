namespace SixOsTL.Application.DTOs.TaiLieu
{
    public record LichSuXemVideoDto(
        long Id,
        long IDVideo,
        string TenVideo,
        long IDTaiKhoanDT,
        string TenTaiKhoan,
        int? Phut,
        int? Giay
    );
}
