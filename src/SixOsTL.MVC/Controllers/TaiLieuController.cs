using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixOsTL.Application.Common.Interfaces;
using SixOsTL.Application.DTOs.DanhMuc;
using SixOsTL.Application.DTOs.TaiLieu;
using SixOsTL.MVC.Extensions;

namespace SixOsTL.MVC.Controllers;

public class TaiLieuController : Controller
{
    private readonly ITaiLieuService _service;
    private readonly IApplicationDbContext _db;
    private readonly IFtpService _ftp;

    public TaiLieuController(ITaiLieuService service, IApplicationDbContext db, IFtpService ftp)
    {
        _service = service;
        _db = db;
        _ftp = ftp;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var chucNangs = await _db.ChucNangs
            .Where(c => c.Active)
            .OrderBy(c => c.IDSanPham)
            .Select(c => new ChucNangDto(
                c.Id, c.IDSanPham, c.SanPham.TenSP,
                c.ChucNang, c.DuongDanFile,
                c.MucDoUuTien != null ? c.MucDoUuTien.MucDo : null))
            .ToListAsync(ct);
        var cnIds = chucNangs.Select(c => c.Id).ToList();

        var allVideos = await _db.Videos
            .Where(v => cnIds.Contains(v.IDChucNang) && v.Active)
            .OrderBy(v => v.STT)
            .Select(v => new VideoDto(
                v.Id, v.STT, v.IDChucNang, null,
                v.TenVideo, v.Keyword, v.DuongDanFileVideo))
            .ToListAsync(ct);

        var allFiles = await _db.Files
            .Where(f => cnIds.Contains(f.IDChucNang) && f.Active)
            .OrderBy(f => f.STT)
            .Select(f => new FileDto(
                f.Id, f.STT, f.IDChucNang, null,
                f.TenFile, f.Keyword, f.DuongDanFile))
            .ToListAsync(ct);

        ViewBag.VideosByChucNang = allVideos.GroupBy(v => v.IDChucNang).ToDictionary(g => g.Key, g => g.AsEnumerable());
        ViewBag.FilesByChucNang = allFiles.GroupBy(f => f.IDChucNang).ToDictionary(g => g.Key, g => g.AsEnumerable());

        return View(chucNangs);
    }

    [HttpGet]
    public async Task<IActionResult> StreamFile(string path, string fileName, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(path)) return BadRequest();
        var ext = Path.GetExtension(path).ToLowerInvariant() is { Length: > 0 } e1 ? e1 : Path.GetExtension(fileName ?? "").ToLowerInvariant();
        var isVideo = ext is ".mp4" or ".webm" or ".mov" or ".avi";
        var contentType = ext switch
        {
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
        // ── VIDEO: cache local rồi serve ─────────────────────────
        if (isVideo)
        {
            var cacheDir = Path.Combine(Path.GetTempPath(), "sixos_video_cache");
            var cacheKey = Convert.ToHexString(
                                System.Security.Cryptography.MD5.HashData(
                                    System.Text.Encoding.UTF8.GetBytes(path)));
            var cachePath = Path.Combine(cacheDir, cacheKey + ext);

            Directory.CreateDirectory(cacheDir);

            if (!System.IO.File.Exists(cachePath))
            {
                var tempPath = cachePath + ".tmp";
                var result = await _ftp.DownloadToLocalPathAsync(path, tempPath, ct);//
                if (!result.Success)
                {
                    if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
                    return NotFound();
                }
                System.IO.File.Move(tempPath, cachePath, overwrite: true);
            }

            return PhysicalFile(cachePath, contentType, enableRangeProcessing: true);
        }

        // ── PDF / WORD: download stream như cũ ───────────────────
        var stream = await _ftp.DownloadToStreamAsync(path, ct);
        if (stream is null) return NotFound();

        if (ext == ".pdf") return File(stream, contentType);

        return File(stream, contentType, fileName);
    }

    [HttpGet]
    public async Task<IActionResult> GetHoiDap(long idChucNang, CancellationToken ct)
    {
        bool isAdmin = HttpContext.Session.IsAdmin();
        var list = await _service.GetHoiDapsByChucNangAsync(idChucNang, !isAdmin, ct);
        return Json(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuiCauHoi(long idChucNang, string noiDung, bool congKhai, long? parentId, CancellationToken ct)
    {
        var userId = HttpContext.Session.GetUserId();
        if (userId is null) return Unauthorized();
        await _service.CreateHoiDapAsync(new CreateHoiDapDto(idChucNang, userId.Value, noiDung, congKhai, parentId), ct);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetVideoLienQuan(long idVideo, CancellationToken ct)
    {
        var list = await _service.GetVideoLienQuanAsync(idVideo, ct);
        return Json(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVideoLienQuan([FromBody] UpsertVideoLienQuanDto dto, CancellationToken ct)
    {
        if (!HttpContext.Session.IsAdmin()) return Forbid();
        var result = await _service.AddVideoLienQuanAsync(dto, ct);
        return Json(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVideoLienQuan(long id, CancellationToken ct)
    {
        if (!HttpContext.Session.IsAdmin()) return Forbid();
        await _service.DeleteVideoLienQuanAsync(id, ct);
        return Ok();
    }
}