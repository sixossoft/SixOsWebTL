using SixOsTL.Application.DTOs.Ftp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixOsTL.Application.Common.Interfaces
{
    public interface IFtpService
    {
        #region UPLOAD
        // Upload từ stream (phù hợp IFormFile từ MVC)
        Task<FtpResult> UploadAsync(
            Stream fileStream,
            string remotePath,          // VD: "/videos/chucnang_1/huong_dan.mp4"
            bool overwrite = false,
            IProgress<double>? progress = null,
            CancellationToken ct = default);
        // Upload từ đường dẫn file cục bộ
        Task<FtpResult> UploadFromLocalPathAsync(string localPath, string remotePath, bool overwrite = false, CancellationToken ct = default);
        #endregion

        #region DOWNLOAD
        // Download về stream (để stream thẳng ra browser)
        Task<Stream?> DownloadToStreamAsync(string remotePath, CancellationToken ct = default);
        // Download về file cục bộ
        Task<FtpResult> DownloadToLocalPathAsync(string remotePath, string localPath, CancellationToken ct = default);
        #endregion

        #region QUẢN LÝ FILE
        Task<bool> FileExistsAsync(string remotePath, CancellationToken ct = default);
        Task<FtpResult> DeleteFileAsync(string remotePath, CancellationToken ct = default);
        Task<FtpResult> RenameAsync(string oldPath, string newPath, CancellationToken ct = default);
        Task<FtpResult> MoveAsync(string sourcePath, string destPath, CancellationToken ct = default);
        #endregion

        #region QUẢN LÝ THƯ MỤC
        Task<FtpResult> CreateDirectoryAsync(string remotePath, CancellationToken ct = default);
        Task<bool> DirectoryExistsAsync(string remotePath, CancellationToken ct = default);
        Task<FtpResult> DeleteDirectoryAsync(string remotePath, CancellationToken ct = default);    
        Task<IEnumerable<FtpFileInfo>> ListDirectoryAsync(string remotePath, CancellationToken ct = default); // Liệt kê file/thư mục trong một thư mục
        Task<IEnumerable<FtpFolderInfo>> ListFoldersAsync(string remotePath, CancellationToken ct = default);
        #endregion

        #region UTILITY
        Task<bool> PathExistsAsync(string remotePath, CancellationToken ct = default);
        #endregion

        #region STREAMING VIDEO
        Task<(Stream? stream, long size)> OpenReadStreamAsync(string remotePath, long offset = 0, CancellationToken ct = default);
        #endregion

        #region TIỆN ÍCH
        // Sinh đường dẫn FTP chuẩn từ các segment, tự tạo thư mục nếu chưa có
        Task<string> EnsureDirectoryAndGetPathAsync(string baseDirectory, params string[] segments);
        // Lấy URL công khai (nếu FTP server có HTTP access).
        string GetPublicUrl(string remotePath);
        #endregion
    }
}
