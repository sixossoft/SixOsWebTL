namespace SixOsTL.Application.DTOs.TaiLieu
{
    public record FileDto(
        long Id,
        string? STT,
        long IDChucNang,
        string? TenChucNang,
        string? TenFile,
        string? Keyword,
        string? DuongDanFile
    );
}
