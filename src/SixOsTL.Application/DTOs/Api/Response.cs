namespace SixOsTL.Application.DTOs.Api
{
    public record PingResponse(bool Success, string Message, int ItemCount, DateTime Timestamp);
    public record ListResponse(string Path, IEnumerable<FileItemDto> Items);
    public record FileItemDto(string Name, string FullPath, string Type, string? Size, string Modified);
    public record UploadResponse(bool Success, string? RemotePath, string? PublicUrl, string FileSize, string? Error);
    public record ExistsResponse(string Path, bool Exists);
    public record DeleteResponse(bool Success, string Path, string? Error);
}
