using Microsoft.EntityFrameworkCore;
using SixOsTL.Domain.Entities;

namespace SixOsTL.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<ThongTinDoanhNghiepDaoTao> ThongTinDoanhNghieps { get; }
        DbSet<TaiKhoanDaoTao> TaiKhoans { get; }
        DbSet<DmVaiTro> VaiTros { get; }
        DbSet<TaiKhoanVaiTro> TaiKhoanVaiTros { get; }
        DbSet<TaiKhoanChucNang> TaiKhoanChucNangs { get; }
        DbSet<DmVaiTroChucNang> VaiTroChucNangs { get; }
        DbSet<DmSanPham> SanPhams { get; }
        DbSet<DmMucDoUuTien> MucDoUuTiens { get; }
        DbSet<DmChucNang> ChucNangs { get; }
        DbSet<TaiLieuVideo> Videos { get; }
        DbSet<TaiLieuFile> Files { get; }
        DbSet<TaiLieuHoiDap> HoiDaps { get; }
        DbSet<TaiLieuVideoTag> VideoTags { get; }
        DbSet<TaiLieuVideoTagMap> VideoTagMaps { get; }
        DbSet<TaiLieuVideoLienQuan> VideoLienQuans { get; }
        DbSet<LichSuXemVideo> LichSuXemVideos { get; }
        DbSet<TaiLieuHoiDapHinhAnh> TaiLieuHoiDapHinhAnhs { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}