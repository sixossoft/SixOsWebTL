namespace SixOsTL.Application.DTOs.TaiLieu
{
    public record CreateVideoDto(
       long IDChucNang,
       string? STT,
       string TenVideo,
       string? Keyword,
       string DuongDanFileVideo
   );
}
