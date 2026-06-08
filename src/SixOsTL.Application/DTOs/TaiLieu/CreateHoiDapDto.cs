namespace SixOsTL.Application.DTOs.TaiLieu
{
    public record CreateHoiDapDto(
        long IDChucNang,
        long IDTaiKhoan,
        string NoiDung,
        bool CongKhai,
        long? ParentHoiDapID,
        List<string>? DanhSachAnhs = null
    );
}
