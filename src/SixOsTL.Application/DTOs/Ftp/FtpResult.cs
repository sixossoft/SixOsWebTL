namespace SixOsTL.Application.DTOs.Ftp
{
    public record FtpResult( // Kết quả trả về sau mỗi thao tác FTP.
       bool Success,
       string? RemotePath,
       string? ErrorMessage
    )
    {
        public static FtpResult Ok(string remotePath) => new(true, remotePath, null);
        public static FtpResult Fail(string error) => new(false, null, error);
    }
}
