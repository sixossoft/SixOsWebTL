namespace SixOsTL.Application.DTOs.Auth
{
    public record LoginResultDto(
        long Id,
        string TenTK,
        string? HoTen,
        string MaCSKCB,
        IEnumerable<string> Roles,
        string? Email
    );
}
