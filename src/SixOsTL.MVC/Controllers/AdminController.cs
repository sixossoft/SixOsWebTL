using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using SixOsTL.Application.Common.Interfaces;
using SixOsTL.Application.DTOs.DanhMuc;
using SixOsTL.Application.DTOs.TaiKhoan;
using SixOsTL.Application.DTOs.TaiLieu;
using SixOsTL.Domain.Entities;
using SixOsTL.Infrastructure.Helpers;
using SixOsTL.MVC.Extensions;
using SixOsTL.MVC.Models;

namespace SixOsTL.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IApplicationDbContext _db;
        private readonly ITaiLieuService _taiLieu;
        private readonly IFtpService _ftp;

        public AdminController(IApplicationDbContext db, ITaiLieuService taiLieu, IFtpService ftp)
        {
            _db = db;
            _taiLieu = taiLieu;
            _ftp = ftp;
        }

        // ── Guard ─────────────────────────────────────────────────
        private IActionResult? GuardAdmin() => HttpContext.Session.IsAdmin() ? null : RedirectToAction("Login", "Account");

        private async Task LoadChucNangDropdown(CancellationToken ct) =>
            ViewBag.ChucNangs = await _db.ChucNangs
                .OrderBy(c => c.IDSanPham).ThenBy(c => c.ChucNang)
                .Select(c => new ChucNangDto(c.Id, c.IDSanPham, c.SanPham.TenSP, c.ChucNang, null, null))
                .ToListAsync(ct);

        #region DASHBOARD
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            ViewData["ActiveMenu"] = "dashboard";

            var totalVideos = await _db.Videos.CountAsync(v => v.Active, ct);
            var totalFiles = await _db.Files.CountAsync(f => f.Active, ct);
            var totalUsers = await _db.TaiKhoans.CountAsync(t => !t.IsDeleted, ct);
            var pendingCount = await _db.HoiDaps.CountAsync(h => h.Active && h.ParentHoiDapID == null && !h.TraLois.Any(r => r.Active), ct);

            ViewBag.Stats = new AdminStatsViewModel
            {
                TotalVideos = totalVideos,
                TotalFiles = totalFiles,
                TotalUsers = totalUsers,
                PendingHoiDap = pendingCount
            };
            ViewBag.PendingCount = pendingCount;
            ViewBag.HoTen = HttpContext.Session.GetString("HoTen") ?? HttpContext.Session.GetString("TenTK");

            ViewBag.Pending = await _db.HoiDaps
                .Where(h => h.Active && h.ParentHoiDapID == null && !h.TraLois.Any(r => r.Active))
                .Include(h => h.TaiKhoan)
                .OrderByDescending(h => h.NgayTao).Take(6)
                .Select(h => new HoiDapDto(h.Id, h.IDChucNang, h.IDTaiKhoan,
                    h.TaiKhoan.HoTen ?? h.TaiKhoan.TenTK,
                    h.NoiDung, h.CongKhai, h.Active, null, h.NgayTao,
                    Enumerable.Empty<HoiDapDto>(),
                    Enumerable.Empty<HoiDapHinhAnhDto>()))
                .ToListAsync(ct);

            ViewBag.RecentVideos = await _db.Videos.Where(v => v.Active)
                .OrderByDescending(v => v.Id).Take(3)
                .Select(v => new VideoDto(v.Id, v.STT, v.IDChucNang, null, v.TenVideo, v.Keyword, v.DuongDanFileVideo))
                .ToListAsync(ct);

            ViewBag.RecentFiles = await _db.Files.Where(f => f.Active)
                .OrderByDescending(f => f.Id).Take(3)
                .Select(f => new FileDto(f.Id, f.STT, f.IDChucNang, null, f.TenFile, f.Keyword, f.DuongDanFile))
                .ToListAsync(ct);

            return View();
        }
        #endregion

        #region QUẢN LÝ VIDEO
        public async Task<IActionResult> QuanLyVideo(CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            ViewData["ActiveMenu"] = "video";
            ViewBag.PendingCount = await PendingCount(ct);

            var chucNangs = await _db.ChucNangs
                .OrderBy(c => c.IDSanPham).ThenBy(c => c.ChucNang)
                .Select(c => new ChucNangDto(c.Id, c.IDSanPham, c.SanPham.TenSP, c.ChucNang, null, null))
                .ToListAsync(ct);

            var cnIds = chucNangs.Select(c => c.Id).ToList();
            var allVideos = await _db.Videos
                .Where(v => cnIds.Contains(v.IDChucNang))
                .OrderBy(v => v.STT)
                .Select(v => new VideoDto(v.Id, v.STT, v.IDChucNang, null, v.TenVideo, v.Keyword, v.DuongDanFileVideo))
                .ToListAsync(ct);

            ViewBag.VideosByChucNang = allVideos.GroupBy(v => v.IDChucNang).ToDictionary(g => g.Key, g => g.AsEnumerable());
            await LoadChucNangDropdown(ct);
            return View(chucNangs);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(500 * 1024 * 1024)]
        public async Task<IActionResult> UploadVideo(long idChucNang, IFormFile file, string tenVideo, string? keyword, string? stt, string? remoteFolder, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            if (file is null || file.Length == 0) return BadRequest("Chưa chọn file.");

            var slug = SlugHelper.ToSlug(tenVideo);
            var ext = Path.GetExtension(file.FileName);
            var targetFolder = string.IsNullOrWhiteSpace(remoteFolder) ? $"cn_{idChucNang}" : remoteFolder.Trim().Trim('/');
            var remotePath = $"{targetFolder}/{slug}{ext}";
            await using var stream = file.OpenReadStream();
            var result = await _ftp.UploadAsync(stream, remotePath, ct: ct);
            if (!result.Success) return BadRequest(result.ErrorMessage);

            await _taiLieu.CreateVideoAsync(new CreateVideoDto(idChucNang, stt, tenVideo, keyword, result.RemotePath!), ct);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetFtpFolders(string? path, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            var currentPath = string.IsNullOrWhiteSpace(path) ? string.Empty : path.Trim().Trim('/');
            var folders = await _ftp.ListFoldersAsync(currentPath, ct);
            return Json(new { path = currentPath, folders });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaVideo(long id, string? remotePath, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            if (!string.IsNullOrEmpty(remotePath)) await _ftp.DeleteFileAsync(remotePath, ct);
            await _taiLieu.DeleteVideoAsync(id, ct);
            return RedirectToAction(nameof(QuanLyVideo));
        }

        // ── VIDEO LIÊN QUAN ───────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetVideoLienQuan(long idVideo, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            var result = await _db.VideoLienQuans
                .Where(lq => lq.IDVideo == idVideo && lq.Active)
                .Include(lq => lq.VideoLienQuan)
                .OrderBy(lq => lq.STT)
                .Select(lq => new {
                    lienQuanId = lq.Id,            // ID bản ghi VideoLienQuan (dùng để xóa)
                    id = lq.VideoLienQuan.Id,
                    tenVideo = lq.VideoLienQuan.TenVideo,
                    stt = lq.STT,
                    idTag = lq.IDTag
                })
                .ToListAsync(ct);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> SearchVideos(string? q, long excludeId, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            var query = _db.Videos.Where(v => v.Id != excludeId);
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(v => v.TenVideo.Contains(q) || (v.Keyword != null && v.Keyword.Contains(q)));
            var result = await query
                .OrderBy(v => v.IDChucNang).ThenBy(v => v.STT)
                .Take(30)
                .Select(v => new { v.Id, v.TenVideo, TenChucNang = v.ChucNang.ChucNang, v.STT })
                .ToListAsync(ct);
            return Json(result);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemVideoLienQuan(long idVideo, long idVideoLienQuan, int stt, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            var dto = new UpsertVideoLienQuanDto(idVideo, idVideoLienQuan, null, stt);
            var result = await _taiLieu.AddVideoLienQuanAsync(dto, ct);
            return Json(result);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaVideoLienQuan(long id, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            await _taiLieu.DeleteVideoLienQuanAsync(id, ct);
            return Ok();
        }
        #endregion

        #region QUẢN LÝ FILE
        public async Task<IActionResult> QuanLyFile(CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            ViewData["ActiveMenu"] = "file";
            ViewBag.PendingCount = await PendingCount(ct);

            var chucNangs = await _db.ChucNangs
                .OrderBy(c => c.IDSanPham).ThenBy(c => c.ChucNang)
                .Select(c => new ChucNangDto(c.Id, c.IDSanPham, c.SanPham.TenSP, c.ChucNang, null, null))
                .ToListAsync(ct);

            var cnIds = chucNangs.Select(c => c.Id).ToList();
            var allFiles = await _db.Files
                .Where(f => cnIds.Contains(f.IDChucNang))
                .OrderBy(f => f.STT)
                .Select(f => new FileDto(f.Id, f.STT, f.IDChucNang, null, f.TenFile, f.Keyword, f.DuongDanFile))
                .ToListAsync(ct);

            ViewBag.FilesByChucNang = allFiles.GroupBy(f => f.IDChucNang).ToDictionary(g => g.Key, g => g.AsEnumerable());
            await LoadChucNangDropdown(ct);
            return View(chucNangs);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> UploadFile(long idChucNang, IFormFile file, string tenFile, string? keyword, string? stt, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            if (file is null || file.Length == 0) return BadRequest("Chưa chọn file.");

            var slug = SlugHelper.ToSlug(tenFile);
            var ext = Path.GetExtension(file.FileName);
            var remotePath = await _ftp.EnsureDirectoryAndGetPathAsync("files", $"cn_{idChucNang}")
                             + $"/{slug}{ext}";

            await using var stream = file.OpenReadStream();
            var result = await _ftp.UploadAsync(stream, remotePath, ct: ct);
            if (!result.Success) return BadRequest(result.ErrorMessage);

            await _taiLieu.CreateFileAsync(new CreateVideoDto(idChucNang, stt, tenFile, keyword, result.RemotePath!), ct);
            return Ok();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaFile(long id, string? remotePath, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            if (!string.IsNullOrEmpty(remotePath)) await _ftp.DeleteFileAsync(remotePath, ct);
            await _taiLieu.DeleteFileAsync(id, ct);
            return RedirectToAction(nameof(QuanLyFile));
        }
        #endregion

        #region QUẢN LÝ CHỨC NĂNG
        public async Task<IActionResult> QuanLyChucNang(CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            ViewData["ActiveMenu"] = "chucnang";
            ViewBag.PendingCount = await PendingCount(ct);

            var cns = await _db.ChucNangs
                .Select(c => new ChucNangDto(c.Id, c.IDSanPham, c.SanPham.TenSP,
                    c.ChucNang, c.DuongDanFile,
                    c.MucDoUuTien != null ? c.MucDoUuTien.MucDo : null))
                .ToListAsync(ct);

            ViewBag.SanPhams = await _db.SanPhams
                .Select(s => new { s.Id, s.TenSP }).ToListAsync(ct);
            ViewBag.MucDos = await _db.MucDoUuTiens
                .Select(m => new { m.Id, m.MucDo }).ToListAsync(ct);

            return View(cns);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoChucNang(long idSanPham, string chucNang, long? idMucDoUuTien, string? duongDanFile, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            _db.ChucNangs.Add(new DmChucNang
            {
                IDSanPham = idSanPham,
                ChucNang = chucNang,
                IDMucDoUuTien = idMucDoUuTien,
                DuongDanFile = duongDanFile,
                Active = true
            });
            await _db.SaveChangesAsync(ct);
            //return RedirectToAction(nameof(QuanLyChucNang));
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetChucNang(long id, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            var cn = await _db.ChucNangs
                .Where(c => c.Id == id)
                .Select(c => new {
                    c.Id,
                    c.IDSanPham,
                    c.ChucNang,
                    c.IDMucDoUuTien,
                    c.DuongDanFile
                })
                .FirstOrDefaultAsync(ct);
            if (cn == null) return NotFound();
            return Json(cn);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaChucNang(long id, long idSanPham, string chucNang, long? idMucDoUuTien, string? duongDanFile, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            var cn = await _db.ChucNangs.FindAsync(new object[] { id }, ct);
            if (cn == null) return NotFound();
            cn.IDSanPham = idSanPham;
            cn.ChucNang = chucNang;
            cn.IDMucDoUuTien = idMucDoUuTien;
            cn.DuongDanFile = duongDanFile;
            await _db.SaveChangesAsync(ct);
            return Ok();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaChucNang(long id, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            var cn = await _db.ChucNangs.FindAsync(new object[] { id }, ct);
            if (cn != null) { cn.Active = false; await _db.SaveChangesAsync(ct); }
            return RedirectToAction(nameof(QuanLyChucNang));
        }
        #endregion

        #region QUẢN LÝ USER
        public async Task<IActionResult> QuanLyUser(CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            ViewData["ActiveMenu"] = "user";
            ViewBag.PendingCount = await PendingCount(ct);

            var users = await _db.TaiKhoans
                .Where(t => !t.IsDeleted)
                .Include(t => t.TaiKhoanVaiTros).ThenInclude(tv => tv.VaiTro)
                .OrderBy(t => t.MaCSKCB).ThenBy(t => t.TenTK)
                .Select(t => new TaiKhoanDto(
                    t.Id, t.TenTK, t.HoTen, t.MaCSKCB,
                    t.SoDienThoai, t.Email, t.NgayBatDau, t.NgayKetThuc,
                    t.TaiKhoanVaiTros.Select(tv => tv.VaiTro.MaVaiTro)))
                .ToListAsync(ct);

            return View(users);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoTaiKhoan(
            string maCSKCB, string? hoTen, string tenTK, string matKhau,
            string? soDienThoai, string? email, string maVaiTros,
            DateTime? ngayBatDau, DateTime? ngayKetThuc, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;

            var tk = new TaiKhoanDaoTao
            {
                MaCSKCB = maCSKCB,
                HoTen = hoTen,
                TenTK = tenTK,
                MatKhau = matKhau,
                SoDienThoai = soDienThoai,
                Email = email,
                NgayBatDau = ngayBatDau,
                NgayKetThuc = ngayKetThuc,
                IsDeleted = false
            };
            _db.TaiKhoans.Add(tk);
            await _db.SaveChangesAsync(ct);

            var role = await _db.VaiTros.FirstOrDefaultAsync(v => v.MaVaiTro == maVaiTros, ct);
            if (role != null)
            {
                _db.TaiKhoanVaiTros.Add(new TaiKhoanVaiTro
                { IDTaiKhoan = tk.Id, IDVaiTro = role.Id });
                await _db.SaveChangesAsync(ct);
            }
            //return RedirectToAction(nameof(QuanLyUser));
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetTaiKhoan(long id, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            var tk = await _db.TaiKhoans
                .Where(t => t.Id == id)
                .Include(t => t.TaiKhoanVaiTros).ThenInclude(tv => tv.VaiTro)
                .Select(t => new {
                    t.Id,
                    t.TenTK,
                    t.HoTen,
                    t.MaCSKCB,
                    t.SoDienThoai,
                    t.Email,
                    NgayBatDau = t.NgayBatDau.HasValue
                        ? t.NgayBatDau.Value.ToString("yyyy-MM-dd") : null,
                    NgayKetThuc = t.NgayKetThuc.HasValue
                        ? t.NgayKetThuc.Value.ToString("yyyy-MM-dd") : null,
                    MaVaiTro = t.TaiKhoanVaiTros
                        .Select(tv => tv.VaiTro.MaVaiTro).FirstOrDefault()
                })
                .FirstOrDefaultAsync(ct);
            if (tk == null) return NotFound();
            return Json(tk);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaTaiKhoan(
            long id, string maCSKCB, string? hoTen, string tenTK,
            string? matKhau, string? soDienThoai, string? email,
            string maVaiTros, DateTime? ngayBatDau, DateTime? ngayKetThuc,
            CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            var tk = await _db.TaiKhoans
                .Include(t => t.TaiKhoanVaiTros)
                .FirstOrDefaultAsync(t => t.Id == id, ct);
            if (tk == null) return NotFound();

            tk.MaCSKCB = maCSKCB;
            tk.HoTen = hoTen;
            tk.TenTK = tenTK;
            tk.SoDienThoai = soDienThoai;
            tk.Email = email;
            tk.NgayBatDau = ngayBatDau;
            tk.NgayKetThuc = ngayKetThuc;
            if (!string.IsNullOrWhiteSpace(matKhau))
                tk.MatKhau = matKhau;

            // Cập nhật vai trò
            _db.TaiKhoanVaiTros.RemoveRange(tk.TaiKhoanVaiTros);
            var role = await _db.VaiTros.FirstOrDefaultAsync(v => v.MaVaiTro == maVaiTros, ct);
            if (role != null)
                _db.TaiKhoanVaiTros.Add(new TaiKhoanVaiTro
                { IDTaiKhoan = tk.Id, IDVaiTro = role.Id });

            await _db.SaveChangesAsync(ct);
            return Ok();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTaiKhoan(long id, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            var tk = await _db.TaiKhoans.FindAsync(new object[] { id }, ct);
            if (tk != null) { tk.IsDeleted = true; await _db.SaveChangesAsync(ct); }
            return RedirectToAction(nameof(QuanLyUser));
        }
        #endregion

        #region QUẢN LÝ HỎI ĐÁP
        public async Task<IActionResult> QuanLyHoiDap(long? id, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            ViewData["ActiveMenu"] = "hoidap";
            ViewBag.HighlightId = id;

            var pending = await _db.HoiDaps
                .CountAsync(h => h.Active && h.ParentHoiDapID == null
                              && !h.TraLois.Any(r => r.Active), ct);
            ViewBag.PendingCount = pending;

            var list = await _db.HoiDaps
                .Where(h => h.Active && h.ParentHoiDapID == null) // Chỉ lấy Active = true
                .Include(h => h.TaiKhoan)
                .Include(h => h.HinhAnhs)
                .Include(h => h.TraLois).ThenInclude(tr => tr.TaiKhoan)
                .Include(h => h.TraLois).ThenInclude(tr => tr.HinhAnhs)
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync(ct);

            var result = list.Select(h => new HoiDapDto(
                h.Id, h.IDChucNang, h.IDTaiKhoan,
                h.TaiKhoan.HoTen ?? h.TaiKhoan.TenTK,
                h.NoiDung, h.CongKhai, h.Active, h.ParentHoiDapID, h.NgayTao,
                h.TraLois.Where(r => r.Active).Select(r => new HoiDapDto(
                    r.Id, r.IDChucNang, r.IDTaiKhoan,
                    r.TaiKhoan.HoTen ?? r.TaiKhoan.TenTK,
                    r.NoiDung, r.CongKhai, r.Active, r.ParentHoiDapID, r.NgayTao,
                    Enumerable.Empty<HoiDapDto>(),
                    r.HinhAnhs.Select(a => new HoiDapHinhAnhDto(a.Id, a.IdTLHD, a.DuongDanFileAnh)))),
                h.HinhAnhs.Select(a => new HoiDapHinhAnhDto(a.Id, a.IdTLHD, a.DuongDanFileAnh))));

            return View(result);
        }

        public async Task<IActionResult> DanhSachTinAn(CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            ViewData["ActiveMenu"] = "hoidap";

            var list = await _db.HoiDaps
                .Where(h => !h.Active && h.ParentHoiDapID == null) // Chỉ lấy Active = false
                .Include(h => h.TaiKhoan)
                .Include(h => h.HinhAnhs)
                .Include(h => h.TraLois).ThenInclude(tr => tr.TaiKhoan)
                .Include(h => h.TraLois).ThenInclude(tr => tr.HinhAnhs)
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync(ct);

            var result = list.Select(h => new HoiDapDto(
                h.Id, h.IDChucNang, h.IDTaiKhoan,
                h.TaiKhoan.HoTen ?? h.TaiKhoan.TenTK,
                h.NoiDung, h.CongKhai, h.Active, h.ParentHoiDapID, h.NgayTao,
                h.TraLois.Where(r => r.Active).Select(r => new HoiDapDto(
                    r.Id, r.IDChucNang, r.IDTaiKhoan,
                    r.TaiKhoan.HoTen ?? r.TaiKhoan.TenTK,
                    r.NoiDung, r.CongKhai, r.Active, r.ParentHoiDapID, r.NgayTao,
                    Enumerable.Empty<HoiDapDto>(),
                    r.HinhAnhs.Select(a => new HoiDapHinhAnhDto(a.Id, a.IdTLHD, a.DuongDanFileAnh)))),
                h.HinhAnhs.Select(a => new HoiDapHinhAnhDto(a.Id, a.IdTLHD, a.DuongDanFileAnh))));

            return View(result);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(100 * 1024 * 1024)]
        public async Task<IActionResult> TraLoiHoiDap(long parentId, long idChucNang, string noiDung, List<IFormFile>? hinhAnhs, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            var userId = HttpContext.Session.GetUserId();
            if (userId is null) return Unauthorized();

            var remotePaths = new List<string>();
            if (hinhAnhs is not null)
            {
                foreach (var file in hinhAnhs.Where(f => f is not null && f.Length > 0))
                {
                    var ext = Path.GetExtension(file.FileName);
                    var fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
                    var remoteDir = await _ftp.EnsureDirectoryAndGetPathAsync("TLSixos", "anh");
                    var remotePath = $"{remoteDir.TrimEnd('/')}/{fileName}";
                    await using var stream = file.OpenReadStream();
                    var result = await _ftp.UploadAsync(stream, remotePath, overwrite: true, ct: ct);
                    if (!result.Success) return BadRequest(result.ErrorMessage);
                    remotePaths.Add(remotePath);
                }
            }

            await _taiLieu.CreateHoiDapAsync(new CreateHoiDapDto(idChucNang, userId.Value, noiDung, true, parentId, remotePaths), ct);
            return RedirectToAction(nameof(QuanLyHoiDap));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHoiDap(long id, CancellationToken ct)
        {
            if (GuardAdmin() is { } r) return r;
            await _taiLieu.ToggleActiveHoiDapAsync(id, ct);
            return RedirectToAction(nameof(QuanLyHoiDap));
        }

        // ── Private helpers ───────────────────────────────────────
        private Task<int> PendingCount(CancellationToken ct) => _db.HoiDaps.CountAsync(h => h.Active && h.ParentHoiDapID == null && !h.TraLois.Any(r => r.Active), ct);
        #endregion
    }
}