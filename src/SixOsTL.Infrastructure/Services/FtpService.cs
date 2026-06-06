using FluentFTP;
using FluentFTP.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixOsTL.Application.Common.Interfaces;
using SixOsTL.Infrastructure.Settings;
using AppFtpFileInfo = SixOsTL.Application.DTOs.Ftp.FtpFileInfo;
using AppFtpResult = SixOsTL.Application.DTOs.Ftp.FtpResult;

namespace SixOsTL.Infrastructure.Services;

public class FtpService : IFtpService
{
    private readonly FtpSettings _settings;
    private readonly ILogger<FtpService> _logger;

    public FtpService(IOptions<FtpSettings> settings, ILogger<FtpService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    // ── Factory ──────────────────────────────────────────────
    private async Task<AsyncFtpClient> CreateConnectedClientAsync(CancellationToken ct)
    {
        var client = new AsyncFtpClient(_settings.FtpHost,  _settings.FtpUsername, _settings.FtpPassword, _settings.Port);
        client.Config.RetryAttempts = _settings.RetryCount;
        client.Config.ConnectTimeout = _settings.TimeoutSeconds * 1000;
        client.Config.ReadTimeout = _settings.TimeoutSeconds * 1000;
        client.Config.DataConnectionReadTimeout = _settings.TimeoutSeconds * 1000;
        if (_settings.UseSsl)
        {
            client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
            client.ValidateCertificate += (_, e) => e.Accept = true;
        }
        await client.Connect(ct);
        return client;
    }

    // ── Helpers ──────────────────────────────────────────────
    private string FullPath(string remotePath)
    {
        var base_ = _settings.BaseDirectory.TrimEnd('/');
        var path = remotePath.TrimStart('/');
        return $"{base_}/{path}";
    }

    private static async Task EnsureParentDirectoryAsync(
        AsyncFtpClient client, string remotePath, CancellationToken ct)
    {
        var dir = Path.GetDirectoryName(remotePath)?.Replace('\\', '/');
        if (!string.IsNullOrEmpty(dir))
            await client.CreateDirectory(dir, true, ct);
    }

    private static AppFtpFileInfo ToFileInfo(FtpListItem item) => new(
        item.Name,
        item.FullName,
        item.Size,
        item.Modified,
        item.Type == FtpObjectType.Directory
    );

    // ── UPLOAD ───────────────────────────────────────────────
    public async Task<AppFtpResult> UploadAsync(
        Stream fileStream,
        string remotePath,
        bool overwrite = false,
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        var full = FullPath(remotePath);
        try
        {
            await using var client = await CreateConnectedClientAsync(ct);
            await EnsureParentDirectoryAsync(client, full, ct);
            var mode = overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip;
            IProgress<FluentFTP.FtpProgress>? ftpProgress = progress is not null ? new Progress<FluentFTP.FtpProgress>(p => progress.Report(p.Progress)) : null;

            var status = await client.UploadStream(fileStream, full, mode, true, ftpProgress, ct);

            if (status == FtpStatus.Success)
            {
                _logger.LogInformation("FTP upload OK: {Path}", full);
                return AppFtpResult.Ok(remotePath);
            }

            if (status == FtpStatus.Skipped)
            {
                _logger.LogWarning("FTP upload skipped (file exists): {Path}", full);
                return AppFtpResult.Fail($"File đã tồn tại: {remotePath}. Dùng overwrite=true nếu muốn ghi đè.");
            }

            return AppFtpResult.Fail("Upload thất bại, không rõ nguyên nhân.");
        }
        catch (FtpException ex)
        {
            _logger.LogError(ex, "FTP error khi upload: {Path}", full);
            return AppFtpResult.Fail($"FTP error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xác định khi upload FTP: {Path}", full);
            return AppFtpResult.Fail($"Lỗi: {ex.Message}");
        }
    }

    public async Task<AppFtpResult> UploadFromLocalPathAsync(string localPath, string remotePath, bool overwrite = false, CancellationToken ct = default)
    {
        if (!File.Exists(localPath))
            return AppFtpResult.Fail($"File cục bộ không tồn tại: {localPath}");

        await using var fs = File.OpenRead(localPath);
        return await UploadAsync(fs, remotePath, overwrite, null, ct);
    }

    // ── DOWNLOAD ─────────────────────────────────────────────
    public async Task<Stream?> DownloadToStreamAsync(string remotePath, CancellationToken ct = default)
    {
        var full = FullPath(remotePath);
        try
        {
            await using var client = await CreateConnectedClientAsync(ct);
            var ms = new MemoryStream();
            bool ok = await client.DownloadStream(ms, full, token: ct);
            if (!ok)
            {
                _logger.LogWarning("FTP download thất bại: {Path}", full);
                return null;
            }
            ms.Position = 0;
            return ms;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP download error: {Path}", full);
            return null;
        }
    }

    public async Task<AppFtpResult> DownloadToLocalPathAsync(string remotePath, string localPath, CancellationToken ct = default)
    {
        var full = FullPath(remotePath);
        try
        {
            await using var client = await CreateConnectedClientAsync(ct);
            var dir = Path.GetDirectoryName(localPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            var status = await client.DownloadFile(
                localPath, full, FtpLocalExists.Overwrite, FtpVerify.None, null, ct);

            return status == FtpStatus.Success
                ? AppFtpResult.Ok(localPath)
                : AppFtpResult.Fail("Download thất bại.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP download-to-local error: {Path}", full);
            return AppFtpResult.Fail(ex.Message);
        }
    }

    // ── QUẢN LÝ FILE ─────────────────────────────────────────
    public async Task<bool> FileExistsAsync(string remotePath, CancellationToken ct = default)
    {
        var full = FullPath(remotePath);
        try
        {
            await using var client = await CreateConnectedClientAsync(ct);
            return await client.FileExists(full, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP FileExists error: {Path}", full);
            return false;
        }
    }

    public async Task<AppFtpResult> DeleteFileAsync(string remotePath, CancellationToken ct = default)
    {
        var full = FullPath(remotePath);
        try
        {
            await using var client = await CreateConnectedClientAsync(ct);
            await client.DeleteFile(full, ct);
            _logger.LogInformation("FTP delete OK: {Path}", full);
            return AppFtpResult.Ok(remotePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP delete error: {Path}", full);
            return AppFtpResult.Fail(ex.Message);
        }
    }

    public async Task<AppFtpResult> RenameAsync(string oldPath, string newPath, CancellationToken ct = default)
    {
        try
        {
            await using var client = await CreateConnectedClientAsync(ct);
            await client.Rename(FullPath(oldPath), FullPath(newPath), ct);
            return AppFtpResult.Ok(newPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP rename error: {Old} -> {New}", oldPath, newPath);
            return AppFtpResult.Fail(ex.Message);
        }
    }

    public async Task<AppFtpResult> MoveAsync(string sourcePath, string destPath, CancellationToken ct = default)
    {
        var fullSrc = FullPath(sourcePath);
        var fullDest = FullPath(destPath);
        try
        {
            await using var client = await CreateConnectedClientAsync(ct);
            await EnsureParentDirectoryAsync(client, fullDest, ct);
            bool ok = await client.MoveFile(fullSrc, fullDest, FtpRemoteExists.Overwrite, ct);
            return ok ? AppFtpResult.Ok(destPath) : AppFtpResult.Fail("Move thất bại.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP move error: {Src} -> {Dest}", fullSrc, fullDest);
            return AppFtpResult.Fail(ex.Message);
        }
    }

    // ── QUẢN LÝ THƯ MỤC ─────────────────────────────────────
    public async Task<AppFtpResult> CreateDirectoryAsync(string remotePath, CancellationToken ct = default)
    {
        var full = FullPath(remotePath);
        try
        {
            await using var client = await CreateConnectedClientAsync(ct);
            await client.CreateDirectory(full, true, ct);
            return AppFtpResult.Ok(remotePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP CreateDirectory error: {Path}", full);
            return AppFtpResult.Fail(ex.Message);
        }
    }

    public async Task<bool> DirectoryExistsAsync(string remotePath, CancellationToken ct = default)
    {
        var full = FullPath(remotePath);
        try
        {
            await using var client = await CreateConnectedClientAsync(ct);
            return await client.DirectoryExists(full, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP DirectoryExists error: {Path}", full);
            return false;
        }
    }

    public async Task<AppFtpResult> DeleteDirectoryAsync(string remotePath, CancellationToken ct = default)
    {
        var full = FullPath(remotePath);
        try
        {
            await using var client = await CreateConnectedClientAsync(ct);
            await client.DeleteDirectory(full, ct);
            return AppFtpResult.Ok(remotePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP DeleteDirectory error: {Path}", full);
            return AppFtpResult.Fail(ex.Message);
        }
    }

    public async Task<IEnumerable<AppFtpFileInfo>> ListDirectoryAsync(string remotePath, CancellationToken ct = default)
    {
        var full = FullPath(remotePath);
        try
        {
            await using var client = await CreateConnectedClientAsync(ct);
            var items = await client.GetListing(full, ct);
            return items.Select(ToFileInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP ListDirectory error: {Path}", full);
            return Enumerable.Empty<AppFtpFileInfo>();
        }
    }

    // ── STREAMING VIDEO ─────────────────────────────────────
    public async Task<(Stream? stream, long size)> OpenReadStreamAsync(string remotePath, long offset = 0, CancellationToken ct = default)
    {
        var full = FullPath(remotePath);
        try
        {
            var client = await CreateConnectedClientAsync(ct);
            var size = await client.GetFileSize(full, -1, ct);
            var ftpStream = await client.OpenRead(full, FtpDataType.Binary, offset);
            return (new FtpStreamSession(client, ftpStream), size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP OpenRead error: {Path}", full);
            return (null, -1);
        }
    }

    // ── TIỆN ÍCH ─────────────────────────────────────────────
    public async Task<string> EnsureDirectoryAndGetPathAsync(string baseDirectory, params string[] segments)
    {
        var cleanSegments = segments
            .Select(s => string.Concat(s.Split(Path.GetInvalidFileNameChars())).Trim())
            .Where(s => !string.IsNullOrEmpty(s));

        var relativePath = string.Join("/",
            new[] { baseDirectory.Trim('/') }.Concat(cleanSegments));

        await CreateDirectoryAsync(relativePath);
        return relativePath;
    }

    public string GetPublicUrl(string remotePath)
    {
        var baseUrl = _settings.PublicBaseUrl.TrimEnd('/');
        var path = remotePath.TrimStart('/');
        return string.IsNullOrEmpty(baseUrl) ? path : $"{baseUrl}/{path}";
    }
}