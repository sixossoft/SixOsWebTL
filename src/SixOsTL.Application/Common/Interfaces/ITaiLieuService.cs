using SixOsTL.Application.DTOs.TaiLieu;

namespace SixOsTL.Application.Common.Interfaces
{
    public interface ITaiLieuService
    {
        // Video
        Task<IEnumerable<VideoDto>> GetVideosByChucNangAsync(long idChucNang, CancellationToken ct = default);
        Task<VideoDto?> GetVideoByIdAsync(long id, CancellationToken ct = default);
        Task<VideoDto> CreateVideoAsync(CreateVideoDto dto, CancellationToken ct = default);
        Task DeleteVideoAsync(long id, CancellationToken ct = default);
        Task<IEnumerable<VideoLienQuanDto>> GetVideoLienQuanAsync(long idVideo, CancellationToken ct = default);
        Task<VideoLienQuanDto> AddVideoLienQuanAsync(UpsertVideoLienQuanDto dto, CancellationToken ct = default);
        Task DeleteVideoLienQuanAsync(long id, CancellationToken ct = default);
        Task UpsertLichSuXemVideoAsync(long idVideo, long idTaiKhoanDt, int phut, int giay, CancellationToken ct = default);
        Task<IEnumerable<LichSuXemVideoDto>> GetLichSuXemVideoByUserAsync(long idTaiKhoanDt, CancellationToken ct = default);

        // File
        Task<IEnumerable<FileDto>> GetFilesByChucNangAsync(long idChucNang, CancellationToken ct = default);
        Task<FileDto> CreateFileAsync(CreateVideoDto dto, CancellationToken ct = default);
        Task DeleteFileAsync(long id, CancellationToken ct = default);

        // Hỏi đáp
        Task<IEnumerable<HoiDapDto>> GetHoiDapsByChucNangAsync(long idChucNang, bool chiBietCongKhai, CancellationToken ct = default);
        Task<HoiDapDto> CreateHoiDapAsync(CreateHoiDapDto dto, CancellationToken ct = default);
        Task ToggleActiveHoiDapAsync(long id, CancellationToken ct = default);
    }
}
