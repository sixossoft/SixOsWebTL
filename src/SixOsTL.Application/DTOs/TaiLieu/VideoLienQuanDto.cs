namespace SixOsTL.Application.DTOs.TaiLieu
{
    public record VideoLienQuanDto(
        long Id,
        string? STT,
        long IDChucNang,
        string TenVideo,
        string DuongDanFileVideo,
        long? IDTag,        // null = manual link
        int SttLienQuan,
        bool IsTagBased
    );
}
