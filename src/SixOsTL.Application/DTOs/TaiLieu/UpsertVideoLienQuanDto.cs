namespace SixOsTL.Application.DTOs.TaiLieu
{
    public record UpsertVideoLienQuanDto(
       long IDVideo,
       long IDVideoLienQuan,
       long? IDTag,
       int STT
   );
}
