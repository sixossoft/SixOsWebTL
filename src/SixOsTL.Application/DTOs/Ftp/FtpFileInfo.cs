namespace SixOsTL.Application.DTOs.Ftp
{
    public record FtpFileInfo(
       string Name,
       string FullPath,
       long Size,              // bytes
       DateTime Modified,
       bool IsDirectory
   );
}
