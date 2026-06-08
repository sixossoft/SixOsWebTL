using Microsoft.EntityFrameworkCore;
using SixOsTL.Application.Common.Interfaces;
using SixOsTL.Application.DTOs.TaiLieu;
using SixOsTL.Domain.Entities;
using SixOsTL.Infrastructure.Persistence;

namespace SixOsTL.Infrastructure.Services
{
    public class TaiLieuService : ITaiLieuService
    {
        private readonly SixOsTLDbContext _db;
        public TaiLieuService(SixOsTLDbContext db) => _db = db;

        // ── VIDEO ────────────────────────────────────────────────
        public async Task<IEnumerable<VideoDto>> GetVideosByChucNangAsync(long idChucNang, CancellationToken ct = default)
        {
            return await _db.Videos
                .Where(v => v.IDChucNang == idChucNang && v.Active)
                .OrderBy(v => v.STT)
                .Select(v => new VideoDto(
                    v.Id, v.STT, v.IDChucNang,
                    v.ChucNang.ChucNang,
                    v.TenVideo, v.Keyword, v.DuongDanFileVideo))
                .ToListAsync(ct);
        }

        public async Task<VideoDto?> GetVideoByIdAsync(long id, CancellationToken ct = default)
        {
            return await _db.Videos
                .Where(v => v.Id == id && v.Active)
                .Select(v => new VideoDto(
                    v.Id, v.STT, v.IDChucNang,
                    v.ChucNang.ChucNang,
                    v.TenVideo, v.Keyword, v.DuongDanFileVideo))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<VideoDto> CreateVideoAsync(CreateVideoDto dto, CancellationToken ct = default)
        {
            var entity = new TaiLieuVideo
            {
                IDChucNang = dto.IDChucNang,
                STT = dto.STT,
                TenVideo = dto.TenVideo,
                Keyword = dto.Keyword,
                DuongDanFileVideo = dto.DuongDanFileVideo,
                Active = true
            };
            _db.Videos.Add(entity);
            await _db.SaveChangesAsync(ct);
            return new VideoDto(entity.Id, entity.STT, entity.IDChucNang, null, entity.TenVideo, entity.Keyword, entity.DuongDanFileVideo);
        }

        public async Task DeleteVideoAsync(long id, CancellationToken ct = default)
        {
            var entity = await _db.Videos.FindAsync(new object[] { id }, ct);
            if (entity is not null)
            {
                entity.Active = false;
                await _db.SaveChangesAsync(ct);
            }
        }

        public async Task<IEnumerable<VideoLienQuanDto>> GetVideoLienQuanAsync(long idVideo, CancellationToken ct = default)
        {
            return await _db.VideoLienQuans
                .Where(lq => lq.IDVideo == idVideo && lq.Active)
                .Include(lq => lq.VideoLienQuan)
                .OrderBy(lq => lq.STT)
                .Select(lq => new VideoLienQuanDto(
                    lq.VideoLienQuan.Id,
                    lq.VideoLienQuan.STT,
                    lq.VideoLienQuan.IDChucNang,
                    lq.VideoLienQuan.TenVideo,
                    lq.VideoLienQuan.DuongDanFileVideo,
                    lq.IDTag,
                    lq.STT,
                    lq.IDTag != null
                ))
                .ToListAsync(ct);
        }

        public async Task<VideoLienQuanDto> AddVideoLienQuanAsync(UpsertVideoLienQuanDto dto, CancellationToken ct = default)
        {
            // Upsert: nếu đã tồn tại thì chỉ cập nhật STT / IDTag
            var existing = await _db.VideoLienQuans
                .FirstOrDefaultAsync(lq =>
                    lq.IDVideo == dto.IDVideo &&
                    lq.IDVideoLienQuan == dto.IDVideoLienQuan, ct);

            if (existing is not null)
            {
                existing.IDTag = dto.IDTag;
                existing.STT = dto.STT;
                existing.Active = true;
            }
            else
            {
                existing = new TaiLieuVideoLienQuan
                {
                    IDVideo = dto.IDVideo,
                    IDVideoLienQuan = dto.IDVideoLienQuan,
                    IDTag = dto.IDTag,
                    STT = dto.STT,
                    Active = true
                };
                _db.VideoLienQuans.Add(existing);
            }

            await _db.SaveChangesAsync(ct);

            var related = await _db.Videos.FindAsync(new object[] { dto.IDVideoLienQuan }, ct);
            return new VideoLienQuanDto(
                related!.Id, related.STT, related.IDChucNang,
                related.TenVideo, related.DuongDanFileVideo,
                dto.IDTag, dto.STT, dto.IDTag != null);
        }

        public async Task DeleteVideoLienQuanAsync(long id, CancellationToken ct = default)
        {
            var entity = await _db.VideoLienQuans.FindAsync(new object[] { id }, ct);
            if (entity is not null) { entity.Active = false; await _db.SaveChangesAsync(ct); }
        }

        public async Task UpsertLichSuXemVideoAsync(long idVideo, long idTaiKhoanDt, int phut, int giay, CancellationToken ct = default)
        {
            var entity = await _db.Set<LichSuXemVideo>()
                .FirstOrDefaultAsync(x => x.IDVideo == idVideo && x.IDTaiKhoanDT == idTaiKhoanDt, ct);

            if (entity is null)
            {
                entity = new LichSuXemVideo
                {
                    IDVideo = idVideo,
                    IDTaiKhoanDT = idTaiKhoanDt,
                    Phut = phut,
                    Giay = giay
                };
                _db.Set<LichSuXemVideo>().Add(entity);
            }
            else
            {
                entity.Phut = phut;
                entity.Giay = giay;
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<LichSuXemVideoDto>> GetLichSuXemVideoByUserAsync(long idTaiKhoanDt, CancellationToken ct = default)
        {
            return await _db.Set<LichSuXemVideo>()
                .Where(x => x.IDTaiKhoanDT == idTaiKhoanDt)
                .Include(x => x.Video)
                .Include(x => x.TaiKhoanDaoTao)
                .OrderByDescending(x => x.Id)
                .Select(x => new LichSuXemVideoDto(
                    x.Id,
                    x.IDVideo,
                    x.Video!.TenVideo ?? "Không rõ",
                    x.IDTaiKhoanDT,
                    x.TaiKhoanDaoTao!.TenTK ?? x.TaiKhoanDaoTao!.HoTen ?? "Người dùng",
                    x.Phut,
                    x.Giay
                ))
                .ToListAsync(ct);
        }

        // ── TAG-BASED: tự động build related list từ tag chung ──────────
        // gọi 1 lần khi admin muốn đồng bộ tag -> VideoLienQuan
        public async Task SyncTagBasedLienQuanAsync(long idVideo, CancellationToken ct = default)
        {
            var myTags = await _db.VideoTagMaps
                .Where(m => m.IDVideo == idVideo)
                .Select(m => m.IDTag)
                .ToListAsync(ct);

            if (!myTags.Any()) return;
            var relatedIds = await _db.VideoTagMaps
                .Where(m => myTags.Contains(m.IDTag) && m.IDVideo != idVideo)
                .Select(m => new { m.IDVideo, m.IDTag })
                .Distinct()
                .ToListAsync(ct);

            foreach (var rel in relatedIds)
            {
                var exists = await _db.VideoLienQuans
                    .AnyAsync(lq => lq.IDVideo == idVideo && lq.IDVideoLienQuan == rel.IDVideo, ct);
                if (!exists)
                {
                    _db.VideoLienQuans.Add(new TaiLieuVideoLienQuan
                    {
                        IDVideo = idVideo,
                        IDVideoLienQuan = rel.IDVideo,
                        IDTag = rel.IDTag,
                        STT = 99,
                        Active = true
                    });
                }
            }
            await _db.SaveChangesAsync(ct);
        }

        // ── FILE ─────────────────────────────────────────────────
        public async Task<IEnumerable<FileDto>> GetFilesByChucNangAsync(long idChucNang, CancellationToken ct = default)
        {
            return await _db.Files
                .Where(f => f.IDChucNang == idChucNang && f.Active)
                .OrderBy(f => f.STT)
                .Select(f => new FileDto(
                    f.Id, f.STT, f.IDChucNang,
                    f.ChucNang.ChucNang,
                    f.TenFile, f.Keyword, f.DuongDanFile))
                .ToListAsync(ct);
        }

        public async Task<FileDto> CreateFileAsync(CreateVideoDto dto, CancellationToken ct = default)
        {
            // dùng chung CreateVideoDto tạm thời, có thể tách CreateFileDto sau
            var entity = new TaiLieuFile
            {
                IDChucNang = dto.IDChucNang,
                STT = dto.STT,
                TenFile = dto.TenVideo,
                Keyword = dto.Keyword,
                DuongDanFile = dto.DuongDanFileVideo,
                Active = true
            };
            _db.Files.Add(entity);
            await _db.SaveChangesAsync(ct);
            return new FileDto(entity.Id, entity.STT, entity.IDChucNang, null, entity.TenFile, entity.Keyword, entity.DuongDanFile);
        }

        public async Task DeleteFileAsync(long id, CancellationToken ct = default)
        {
            var entity = await _db.Files.FindAsync(new object[] { id }, ct);
            if (entity is not null) { entity.Active = false; await _db.SaveChangesAsync(ct); }
        }

        // ── HỎI ĐÁP ─────────────────────────────────────────────
        public async Task<IEnumerable<HoiDapDto>> GetHoiDapsByChucNangAsync(
            long idChucNang, bool chiBietCongKhai, CancellationToken ct = default)
        {
            var query = _db.HoiDaps
                .Where(h => h.IDChucNang == idChucNang
                         && h.Active
                         && h.ParentHoiDapID == null);     // chỉ lấy câu hỏi gốc

            if (chiBietCongKhai) query = query.Where(h => h.CongKhai);

            var list = await query
                .Include(h => h.TaiKhoan)
                .Include(h => h.TraLois)
                    .ThenInclude(r => r.TaiKhoan)
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync(ct);

            return list.Select(MapHoiDap);
        }

        public async Task<HoiDapDto> CreateHoiDapAsync(CreateHoiDapDto dto, CancellationToken ct = default)
        {
            // Chuyển danh sách ảnh thành chuỗi phân cách bằng ';'
            var duongDanAnhs = dto.DanhSachAnhs != null && dto.DanhSachAnhs.Any()
                ? string.Join(";", dto.DanhSachAnhs)
                : null;

            var entity = new TaiLieuHoiDap
            {
                IDChucNang = dto.IDChucNang,
                IDTaiKhoan = dto.IDTaiKhoan,
                NoiDung = dto.NoiDung,
                CongKhai = dto.CongKhai,
                ParentHoiDapID = dto.ParentHoiDapID,
                DuongDanAnhs = duongDanAnhs,
                NgayTao = DateTime.Now,
                Active = true
            };
            _db.HoiDaps.Add(entity);
            await _db.SaveChangesAsync(ct);
            return MapHoiDap(entity);
        }

        public async Task ToggleActiveHoiDapAsync(long id, CancellationToken ct = default)
        {
            var entity = await _db.HoiDaps.FindAsync(new object[] { id }, ct);
            if (entity is not null) { entity.Active = !entity.Active; await _db.SaveChangesAsync(ct); }
        }

        private static HoiDapDto MapHoiDap(TaiLieuHoiDap h)
        {
            // Parse danh sách ảnh từ chuỗi
            var danhSachAnhs = string.IsNullOrEmpty(h.DuongDanAnhs)
                ? Array.Empty<string>()
                : h.DuongDanAnhs.Split(';', StringSplitOptions.RemoveEmptyEntries);

            return new HoiDapDto(
                h.Id, h.IDChucNang, h.IDTaiKhoan,
                h.TaiKhoan?.HoTen ?? h.TaiKhoan?.TenTK,
                h.NoiDung, h.CongKhai, h.ParentHoiDapID, h.NgayTao,
                danhSachAnhs,
                h.TraLois?.Where(r => r.Active).Select(MapHoiDap) ?? Enumerable.Empty<HoiDapDto>()
            );
        }
    }
}
