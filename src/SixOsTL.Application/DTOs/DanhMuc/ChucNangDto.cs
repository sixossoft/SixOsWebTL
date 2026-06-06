namespace SixOsTL.Application.DTOs.DanhMuc
{
    public record ChucNangDto(
        long Id,
        long IDSanPham,
        string? TenSanPham,
        string? ChucNang,
        string? DuongDanFile,
        string? MucDo
    );
}
