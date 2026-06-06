using Microsoft.AspNetCore.Mvc;
using SixOsTL.Application.Common.Interfaces;
using SixOsTL.Application.DTOs.Api;

namespace SixOsTL.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/ftp-test")]
    [Produces("application/json")]
    public class FtpTestController : ControllerBase
    {
        private readonly IFtpService _ftp;

        public FtpTestController(IFtpService ftp) => _ftp = ftp;

        // ── 1. Ping: kiểm tra connect được không ─────────────────
        /// <summary>Kiểm tra kết nối tới FTP server.</summary>
        [HttpGet("ping")]
        [ProducesResponseType(typeof(PingResponse), 200)]
        public async Task<IActionResult> Ping(CancellationToken ct)
        {
            try
            {
                // Thử list root — nếu không exception là connect OK
                var items = await _ftp.ListDirectoryAsync("/", ct);
                return Ok(new PingResponse(
                    Success: true,
                    Message: "Kết nối FTP thành công",
                    ItemCount: items.Count(),
                    Timestamp: DateTime.Now));
            }
            catch (Exception ex)
            {
                return Ok(new PingResponse(
                    Success: false,
                    Message: $"Kết nối thất bại: {ex.Message}",
                    ItemCount: 0,
                    Timestamp: DateTime.Now));
            }
        }

        // ── 2. List: liệt kê thư mục ─────────────────────────────
        /// <summary>Liệt kê file/thư mục tại đường dẫn chỉ định.</summary>
        /// <param name="path">Đường dẫn trên FTP, VD: / hoặc /videos</param>
        [HttpGet("list")]
        [ProducesResponseType(typeof(ListResponse), 200)]
        public async Task<IActionResult> List([FromQuery] string path = "/", CancellationToken ct = default)
        {
            var items = await _ftp.ListDirectoryAsync(path, ct);
            return Ok(new ListResponse(
                Path: path,
                Items: items.Select(i => new FileItemDto(
                    i.Name, i.FullPath,
                    i.IsDirectory ? "directory" : "file",
                    i.IsDirectory ? null : FormatSize(i.Size),
                    i.Modified.ToString("yyyy-MM-dd HH:mm:ss")))));
        }

        // ── 3. Upload: upload file lên FTP ───────────────────────
        /// <summary>Upload file lên FTP server.</summary>
        /// <param name="file">File cần upload</param>
        /// <param name="remotePath">Đường dẫn đích trên FTP, VD: /test/sample.pdf</param>
        /// <param name="overwrite">Ghi đè nếu đã tồn tại</param>
        [HttpPost("upload")]
        [RequestSizeLimit(500 * 1024 * 1024)]
        [ProducesResponseType(typeof(UploadResponse), 200)]
        public async Task<IActionResult> Upload(
            IFormFile file,
            [FromQuery] string remotePath = "/test/",
            [FromQuery] bool overwrite = false,
            CancellationToken ct = default)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { error = "Chưa chọn file hoặc file rỗng." });

            // Nếu remotePath là thư mục (kết thúc /), ghép tên file vào
            var dest = remotePath.EndsWith('/')
                ? $"{remotePath}{file.FileName}"
                : remotePath;

            await using var stream = file.OpenReadStream();
            var result = await _ftp.UploadAsync(stream, dest, overwrite, null, ct);

            return Ok(new UploadResponse(
                Success: result.Success,
                RemotePath: result.RemotePath,
                PublicUrl: result.Success ? _ftp.GetPublicUrl(result.RemotePath!) : null,
                FileSize: FormatSize(file.Length),
                Error: result.ErrorMessage));
        }

        // ── 4. File exists ────────────────────────────────────────
        /// <summary>Kiểm tra file có tồn tại trên FTP không.</summary>
        [HttpGet("exists")]
        [ProducesResponseType(typeof(ExistsResponse), 200)]
        public async Task<IActionResult> Exists(
            [FromQuery] string path, CancellationToken ct)
        {
            var exists = await _ftp.FileExistsAsync(path, ct);
            return Ok(new ExistsResponse(path, exists));
        }

        // ── 5. Delete ─────────────────────────────────────────────
        /// <summary>Xóa file trên FTP (dùng để dọn file test).</summary>
        [HttpDelete("delete")]
        [ProducesResponseType(typeof(DeleteResponse), 200)]
        public async Task<IActionResult> Delete(
            [FromQuery] string path, CancellationToken ct)
        {
            var result = await _ftp.DeleteFileAsync(path, ct);
            return Ok(new DeleteResponse(result.Success, path, result.ErrorMessage));
        }

        // ── 6. Download: stream file về browser ──────────────────
        /// <summary>Download file từ FTP về trình duyệt.</summary>
        [HttpGet("download")]
        public async Task<IActionResult> Download(
            [FromQuery] string path, CancellationToken ct)
        {
            var stream = await _ftp.DownloadToStreamAsync(path, ct);
            if (stream is null)
                return NotFound(new { error = $"Không tìm thấy file: {path}" });

            var fileName = System.IO.Path.GetFileName(path);
            var contentType = GetContentType(System.IO.Path.GetExtension(fileName));
            return File(stream, contentType, fileName);
        }

        // ── Helpers ──────────────────────────────────────────────
        private static string FormatSize(long bytes) => bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
            _ => $"{bytes / (1024.0 * 1024 * 1024):F2} GB"
        };

        private static string GetContentType(string ext) => ext.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}
