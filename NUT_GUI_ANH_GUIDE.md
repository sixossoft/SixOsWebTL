# Hướng Dẫn Nút Gửi Ảnh Trong Hỏi Đáp

## Vấn Đề Hiện Tại
Bạn đang chạy ứng dụng nên không thể build được. Code đã được cập nhật nhưng cần **dừng ứng dụng và build lại** để thấy nút gửi ảnh.

## Các Thay Đổi Đã Thêm

### 1. Cập Nhật Giao Diện (View)
**File**: `src/SixOsTL.MVC/Views/Shared/_AppLayout.cshtml`

Đã thêm:
- Input file ẩn để chọn ảnh
- Nút đính kèm ảnh (icon photo) 
- Khu vực preview ảnh đã chọn

```html
<div class="comment-foot">
    <div class="comment-input-wrap">
        <textarea class="comment-input" id="commentInput"
              placeholder="Nhập câu hỏi của bạn..." rows="3"></textarea>
        <div class="comment-actions">
            <input type="file" id="commentImageInput" accept="image/*" multiple style="display:none;" onchange="handleImageSelect(event)" />
            <button class="btn-attach" onclick="document.getElementById('commentImageInput').click()" aria-label="Đính kèm ảnh" title="Đính kèm ảnh">
                <i class="ti ti-photo"></i>
            </button>
            <button class="btn-send" onclick="submitComment()" aria-label="Gửi">
                <i class="ti ti-send"></i>
            </button>
        </div>
    </div>
    <div class="comment-preview" id="commentPreview" style="display:none;">
        <!-- Hiển thị ảnh preview -->
    </div>
</div>
```

### 2. CSS Mới
**File**: `src/SixOsTL.MVC/wwwroot/dist/css/AppLayout.css`

Đã thêm style cho:
- `.btn-attach`: Nút đính kèm ảnh
- `.comment-preview`: Khu vực preview ảnh
- `.preview-item`: Style cho mỗi ảnh preview
- `.preview-remove`: Nút xóa ảnh
- `.comment-images`: Hiển thị ảnh trong comment bubble
- `.comment-img-item`: Style cho ảnh trong comment

### 3. JavaScript Xử Lý
**File Mới**: `src/SixOsTL.MVC/wwwroot/dist/js/CommentImageHandler.js`

Chức năng:
- `handleImageSelect()`: Xử lý khi chọn ảnh
- `removeImage()`: Xóa ảnh khỏi danh sách
- `renderImagePreview()`: Hiển thị preview ảnh
- `getSelectedImages()`: Lấy danh sách ảnh đã chọn
- `clearSelectedImages()`: Xóa tất cả ảnh

**File Cập Nhật**: `src/SixOsTL.MVC/wwwroot/dist/js/AppLayout.js`

Cập nhật:
- `submitComment()`: Gửi ảnh cùng với text
- `loadComments()`: Hiển thị ảnh trong comments

### 4. Backend (Entity, DTO, Service, Controller)

#### Entity
**File**: `src/SixOsTL.Domain/Entities/TaiLieuHoiDap.cs`
- Thêm trường `DuongDanAnhs` (kiểu string, lưu nhiều đường dẫn phân cách bằng `;`)

#### DTO
**File**: `src/SixOsTL.Application/DTOs/TaiLieu/HoiDapDto.cs`
- Thêm `IEnumerable<string>? DanhSachAnhs`

**File**: `src/SixOsTL.Application/DTOs/TaiLieu/CreateHoiDapDto.cs`
- Thêm `List<string>? DanhSachAnhs`

#### Service
**File**: `src/SixOsTL.Infrastructure/Services/TaiLieuService.cs`
- Cập nhật `CreateHoiDapAsync()`: Lưu danh sách ảnh
- Cập nhật `MapHoiDap()`: Parse danh sách ảnh từ chuỗi

#### Controller
**File**: `src/SixOsTL.MVC/Controllers/TaiLieuController.cs`
- Cập nhật `GuiCauHoi()`: Xử lý upload ảnh lên server

## Cách Xem Nút Gửi Ảnh

### Bước 1: Dừng Ứng Dụng
Bạn cần dừng ứng dụng đang chạy (SixOsTL.MVC process ID 9020)

Trong Visual Studio:
- Nhấn nút Stop (Shift + F5)
- Hoặc đóng cửa sổ browser và dừng debug

### Bước 2: Build Lại
```bash
dotnet build SixOsTL.sln
```

### Bước 3: Chạy Lại Ứng Dụng
```bash
cd src/SixOsTL.MVC
dotnet run
```

Hoặc nhấn F5 trong Visual Studio

### Bước 4: Kiểm Tra
1. Đăng nhập vào hệ thống
2. Vào trang Tài liệu
3. Mở bất kỳ video/file nào
4. Click vào icon **Hỏi đáp** (message circle)
5. Bạn sẽ thấy:
   - Textarea nhập câu hỏi
   - **Nút ảnh (icon photo)** - bên trái
   - Nút gửi (icon send) - bên phải

## Cách Sử Dụng

### Gửi Ảnh
1. Click vào nút **icon photo** 
2. Chọn ảnh từ máy tính (có thể chọn nhiều ảnh, tối đa 5)
3. Ảnh sẽ hiển thị preview phía dưới
4. Nhập nội dung câu hỏi
5. Click nút **Gửi**

### Giới Hạn
- Tối đa **5 ảnh** mỗi câu hỏi
- Kích thước mỗi ảnh tối đa **5MB**
- Chỉ chấp nhận file ảnh (jpg, png, gif, webp, etc.)

### Xóa Ảnh
- Click vào nút **X** (màu đỏ) ở góc phải trên mỗi ảnh preview

## Kiểm Tra Nhanh

Nếu sau khi build và chạy lại mà vẫn không thấy nút ảnh:

1. **Xóa cache browser**:
   - Ctrl + Shift + R (hard refresh)
   - Hoặc Ctrl + F5

2. **Kiểm tra Console**:
   - Mở Developer Tools (F12)
   - Xem tab Console có lỗi JavaScript không

3. **Kiểm tra file CSS đã load**:
   - Mở Developer Tools (F12)
   - Tab Sources → xem file `AppLayout.css`
   - Tìm class `.btn-attach` để đảm bảo CSS mới đã load

4. **Kiểm tra HTML**:
   - Inspect element (F12)
   - Tìm `<button class="btn-attach">`
   - Nếu không thấy, có thể view chưa được build

## Migration Database (Nếu Cần)

Vì đã thêm trường `DuongDanAnhs` vào entity `TaiLieuHoiDap`, bạn cần tạo migration:

```bash
cd src/SixOsTL.Infrastructure
dotnet ef migrations add AddDuongDanAnhsToHoiDap --startup-project ../SixOsTL.MVC
dotnet ef database update --startup-project ../SixOsTL.MVC
```

## Xem Ảnh Trong Comment

Khi có câu hỏi chứa ảnh:
- Ảnh sẽ hiển thị dưới nội dung text
- Click vào ảnh để xem full size (mở tab mới)
- Ảnh sẽ có viền và hover effect

## Troubleshooting

### Nút không hiện
- Kiểm tra file `_AppLayout.cshtml` đã được cập nhật
- Xóa cache browser
- Build lại project

### Không upload được ảnh
- Kiểm tra thư mục `wwwroot/uploads/hoidap` có tồn tại không
- Kiểm tra quyền write vào thư mục
- Xem Console log có lỗi không

### Ảnh không hiển thị
- Kiểm tra đường dẫn ảnh trong database
- Kiểm tra file ảnh đã được upload vào `wwwroot/uploads/hoidap`
- Xem Network tab trong DevTools có lỗi 404 không

## Ghi Chú

Hiện tại mã code đã hoàn chỉnh nhưng bạn đang chạy ứng dụng cũ nên chưa thấy thay đổi.

**Hành động cần thiết**: Dừng app → Build → Chạy lại → Hard refresh browser
