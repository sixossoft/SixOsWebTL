# Hướng Dẫn Tính Năng Lịch Sử Xem Video

## Tổng Quan
Tính năng này cho phép người dùng xem lịch sử xem video của mình, bao gồm thời gian xem (phút và giây) của mỗi video.

## Các Thay Đổi Được Thêm Vào

### 1. DTO Mới: `LichSuXemVideoDto`
**Đường dẫn**: `src/SixOsTL.Application/DTOs/TaiLieu/LichSuXemVideoDto.cs`

Chứa thông tin lịch sử xem video:
- `Id`: Mã định danh
- `IDVideo`: Mã video
- `TenVideo`: Tên video
- `IDTaiKhoanDT`: Mã tài khoản người dùng
- `TenTaiKhoan`: Tên tài khoản
- `Phut`: Số phút đã xem
- `Giay`: Số giây đã xem

### 2. Service Mới: `GetLichSuXemVideoByUserAsync`
**Đường dẫn**: 
- Interface: `src/SixOsTL.Application/Common/Interfaces/ITaiLieuService.cs`
- Implementation: `src/SixOsTL.Infrastructure/Services/TaiLieuService.cs`

**Chức năng**: Lấy danh sách lịch sử xem video của một người dùng.

**Tham số**:
- `idTaiKhoanDt`: Mã tài khoản người dùng
- `ct`: CancellationToken

**Trả về**: Danh sách `LichSuXemVideoDto` được sắp xếp theo thứ tự mới nhất trước.

### 3. API Endpoints Mới

#### Endpoint 1: Lấy Lịch Sử (JSON)
```
GET /TaiLieu/GetLichSuXemVideo
```
- **Yêu cầu xác thực**: Có
- **Trả về**: JSON chứa danh sách lịch sử xem video
- **Mã trạng thái**:
  - 200: Thành công
  - 401: Chưa đăng nhập

#### Endpoint 2: Xem Trang Lịch Sử
```
GET /TaiLieu/LichSuXemVideo
```
- **Yêu cầu xác thực**: Có
- **Trả về**: Trang HTML hiển thị lịch sử xem video
- **Chuyển hướng**: Nếu chưa đăng nhập, chuyển đến trang đăng nhập

### 4. View Mới: `LichSuXemVideo.cshtml`
**Đường dẫn**: `src/SixOsTL.MVC/Views/TaiLieu/LichSuXemVideo.cshtml`

**Tính năng**:
- Hiển thị bảng danh sách các video đã xem
- Hiển thị thời lượng xem (MM:SS) cho mỗi video
- Hiển thị thanh tiến độ dựa trên phần trăm xem
- Hiển thị thống kê: Số video đã xem, tổng thời gian xem
- Nút quay lại để trở về trang tài liệu
- Giao diện thân thiện, responsive

### 5. Cập Nhật Layout
**Đường dẫn**: `src/SixOsTL.MVC/Views/Shared/_AppLayout.cshtml`

**Thay đổi**: Thêm nút "Lịch sử xem video" (icon clock) vào thanh navigation cho người dùng bình thường (không phải admin).

## Cách Sử Dụng

### Cho Người Dùng
1. Đăng nhập vào hệ thống
2. Xem các video trong tài liệu (hệ thống sẽ tự động ghi lại thời gian xem)
3. Nhấp vào icon **Lịch sử xem video** (hình đồng hồ) ở thanh navigation trên cùng
4. Xem danh sách các video đã xem với thời lượng chi tiết

### Cho Nhà Phát Triển
#### Ghi Lịch Sử Xem Video
```csharp
// Khi người dùng xem một video, gọi hàm sau:
await _service.UpsertLichSuXemVideoAsync(
    idVideo: 123,           // Mã video
    idTaiKhoanDt: 456,      // Mã tài khoản
    phut: 5,                // Số phút
    giay: 30                // Số giây
);
```

#### Lấy Lịch Sử Xem Video
```csharp
// Lấy danh sách lịch sử xem video của người dùng
var lichSu = await _service.GetLichSuXemVideoByUserAsync(
    idTaiKhoanDt: 456,      // Mã tài khoản
    ct: cancellationToken
);

// Trả về danh sách LichSuXemVideoDto
```

## Cơ Sở Dữ Liệu

### Table: `LichSuXemVideo`
Bảng này đã tồn tại trong database với các cột:
- `Id` (PK): Khóa chính
- `IDVideo` (FK): Tham chiếu đến bảng Video
- `IDTaiKhoanDT` (FK): Tham chiếu đến bảng TaiKhoanDaoTao
- `Phut`: Số phút xem
- `Giay`: Số giây xem

**Index**:
- Khóa duy nhất trên (IDVideo, IDTaiKhoanDT) để đảm bảo chỉ có một bản ghi lịch sử cho mỗi cặp (video, người dùng)

## Kiểm Thử

### Test Thủ Công
1. Đăng nhập vào hệ thống
2. Xem một video và ghi lại thời gian (ví dụ: 2:45)
3. Truy cập trang lịch sử xem video
4. Kiểm tra xem thời gian hiển thị có khớp không

### API Test
```bash
# Lấy lịch sử xem video (JSON)
curl -X GET http://localhost:5000/TaiLieu/GetLichSuXemVideo \
  -H "Cookie: your-session-cookie"
```

## Lưu Ý

1. **Dữ liệu tồn tại**: Nếu người dùng xem cùng một video lần thứ hai, hệ thống sẽ cập nhật thời gian xem (không tạo bản ghi mới).

2. **Quyền truy cập**: Chỉ người dùng đã đăng nhập mới có thể xem lịch sử của mình.

3. **Hiệu suất**: Danh sách được sắp xếp theo ID giảm dần (video mới nhất lên trên).

4. **Tính toán phần trăm**: Hiện tại, tính toán phần trăm dựa trên giả định rằng thời lượng video tối đa là 0 (để tránh lỗi chia cho 0). Để có độ chính xác cao hơn, bạn có thể:
   - Thêm trường `ThoiLuongPhut` và `ThoiLuongGiay` vào table `TaiLieuVideo`
   - Cập nhật DTO để lấy thông tin này
   - Tính toán phần trăm chính xác

## Mở Rộng Trong Tương Lai

1. **Thống kê chi tiết**: Thêm biểu đồ thống kê thời gian xem theo ngày/tuần/tháng
2. **Lọc nâng cao**: Lọc lịch sử theo chức năng, khoảng thời gian
3. **Xuất dữ liệu**: Xuất lịch sử ra CSV, PDF
4. **Phân tích admin**: Admin có thể xem lịch sử xem video của bất kỳ người dùng nào
5. **Thông báo**: Gửi thông báo khi người dùng xem xong video
