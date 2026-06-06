namespace SixOsTL.Application.DTOs.TaiLieu
{
    public record VideoDto(
        long Id,
        string? STT,
        long IDChucNang,
        string? TenChucNang,
        string? TenVideo,
        string? Keyword,
        string? DuongDanFileVideo
    );
}
