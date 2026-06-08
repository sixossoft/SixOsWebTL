namespace SixOsTL.Application.DTOs.Ftp
{
    public record FtpFolderInfo(
        string Name,
        string FullPath,
        bool HasChildren
    );
}