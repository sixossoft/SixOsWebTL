using SixOsTL.Application.DTOs.Auth;

namespace SixOsTL.Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResultDto?> LoginAsync(string tenTK, string matKhau, CancellationToken ct = default);
        Task<bool> IsInRoleAsync(long idTaiKhoan, string maVaiTro, CancellationToken ct = default);
    }
}
