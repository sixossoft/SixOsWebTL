using Microsoft.EntityFrameworkCore;
using SixOsTL.Application.Common.Interfaces;
using SixOsTL.Application.DTOs.Auth;
using SixOsTL.Infrastructure.Persistence;

namespace SixOsTL.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly SixOsTLDbContext _db;
        public AuthService(SixOsTLDbContext db) => _db = db;

        public async Task<LoginResultDto?> LoginAsync(string tenTK, string matKhau, CancellationToken ct = default)
        {
            var tk = await _db.TaiKhoans
                .Include(t => t.TaiKhoanVaiTros)
                    .ThenInclude(tv => tv.VaiTro)
                .FirstOrDefaultAsync(t => t.TenTK == tenTK && !t.IsDeleted, ct);
            if (tk is null) return null;
            if (tk.MatKhau != matKhau) return null;       
            if (!tk.ConHieuLuc()) return null;
            var roles = tk.TaiKhoanVaiTros.Select(tv => tv.VaiTro.MaVaiTro);
            return new LoginResultDto(tk.Id, tk.TenTK, tk.HoTen, tk.MaCSKCB, roles);
        }

        public async Task<bool> IsInRoleAsync(long idTaiKhoan, string maVaiTro, CancellationToken ct = default)
        {
            return await _db.TaiKhoanVaiTros.AnyAsync(tv => tv.IDTaiKhoan == idTaiKhoan && tv.VaiTro.MaVaiTro == maVaiTro, ct);
        }
    }
}
