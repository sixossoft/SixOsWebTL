# 🔴 HƯỚNG DẪN ĐẦY ĐỦ - GỬI ẢNH TRONG HỎI ĐÁP

## ❗ VẤN ĐỀ HIỆN TẠI
Bạn đang chạy ứng dụng (Process ID: 21864), không thể build code mới.

---

## 📋 BƯỚC 1: DỪNG ỨNG DỤNG

### Option 1: Dùng Visual Studio (Dễ nhất)
1. Mở **Visual Studio 2022**
2. Tìm nút **Stop** (có hình vuông đỏ) trên toolbar
3. Nhấn nó hoặc bấm **Shift + F5**
4. Chờ ứng dụng dừng hoàn toàn (5-10 giây)

### Option 2: Dùng Terminal
```powershell
# Mở PowerShell (Admin)
taskkill /PID 21864 /F
```

### Option 3: Thủ công
1. Mở **Task Manager** (Ctrl + Shift + Esc)
2. Tìm **"dotnet"** hoặc **"SixOsTL"**
3. Click chuột phải → End Task

**✅ Kiểm tra:** Đóng browser, không thấy trang web nữa = ứng dụng đã dừng

---

## 📋 BƯỚC 2: BUILD PROJECT

Mở PowerShell tại thư mục dự án (`D:\SixOsWebTL`) và chạy:

```powershell
dotnet build SixOsTL.sln
```

**Nếu build thành công**, sẽ thấy:
```
Build succeeded.
```

**Nếu build lỗi**, chạy:
```powershell
dotnet clean SixOsTL.sln
dotnet build SixOsTL.sln
```

---

## 📋 BƯỚC 3: CHẠY MIGRATION DATABASE (Nếu cần)

Vì đã thêm trường `DuongDanAnhs`, cần update database:

```powershell
cd src/SixOsTL.Infrastructure
dotnet ef migrations add AddDuongDanAnhsToHoiDap --startup-project ../SixOsTL.MVC
dotnet ef database update --startup-project ../SixOsTL.MVC
```

---

## 📋 BƯỚC 4: CHẠY ỨNG DỤNG

### Option 1: Visual Studio (F5)
- Mở Visual Studio
- Nhấn **F5** hoặc click nút **Run**
- Chờ ứng dụng khởi động

### Option 2: Terminal
```powershell
cd src/SixOsTL.MVC
dotnet run
```

Browser sẽ tự mở hoặc truy cập: **https://localhost:5000**

---

## 📋 BƯỚC 5: KIỂM TRA NÚT GỬI ẢNH

1. **Đăng nhập** vào hệ thống
2. Vào **Tài liệu**
3. Chọn bất kỳ **video** nào
4. Click icon **Hỏi đáp** (message circle bên phải)
5. Bạn sẽ thấy:
   - Textarea nhập câu hỏi
   - **Nút icon photo** ← NÚT GỬI ẢNH
   - Nút icon send (gửi)

**Hình ảnh:**
```
┌─────────────────────────────┐
│ Hỏi đáp                   × │
├─────────────────────────────┤
│ [@username] Vừa xong        │
│ Nội dung câu hỏi...         │
│                             │
├─────────────────────────────┤
│ ┌─────────────────────────┐ │
│ │ Nhập câu hỏi của bạn    │ │
│ │                         │ │
│ └─────────────────────────┘ │
│ [🖼️] [➤]                    │ ← Nút photo & send
└─────────────────────────────┘
```

---

## 📋 BƯỚC 6: GỬI ẢNH

1. **Click nút photo** (🖼️ icon)
2. Chọn 1-5 ảnh từ máy tính
3. Ảnh sẽ hiển thị **preview** phía dưới
4. **Nhập** nội dung câu hỏi
5. **Click nút send** (➤ icon)
6. **Done!** Ảnh + câu hỏi gửi lên server

---

## 🔍 KIỂM TRA LỖI

Nếu không gửi được:

### Mở Developer Tools (F12)

#### Tab "Console"
- Thấy `submitComment: gửi request` = request được gửi ✅
- Thấy `submitComment: response status: 200` = thành công ✅
- Thấy lỗi? Copy dòng lỗi báo cho tôi

#### Tab "Network"
- Click vào request `/TaiLieu/GuiCauHoi`
- Xem **Status**: Phải là **200** hoặc **201**
- Nếu **400** hoặc **500**: Có lỗi server
- Xem **Request**: Có chứa `images` file không?

#### Tab "Application"
- Xem **Local Storage** → Key: `commentPreview`
- Nếu không có: Ảnh chưa được chọn

---

## 🐛 CÁC LỖI THƯỜNG GẶP

### Lỗi 1: Không thấy nút photo
**Nguyên nhân**: CSS chưa load hoặc layout cũ  
**Cách sửa**:
- Ctrl + Shift + R (Hard refresh)
- Xóa browser cache

### Lỗi 2: Ảnh không upload được
**Nguyên nhân**: Thư mục `wwwroot/uploads/hoidap` chưa được tạo  
**Cách sửa**:
```powershell
mkdir src/SixOsTL.MVC/wwwroot/uploads/hoidap
```

### Lỗi 3: Response 400
**Nguyên nhân**: Thiếu anti-forgery token hoặc parameter không đúng  
**Cách sửa**: Mở console xem log chi tiết

### Lỗi 4: Response 500
**Nguyên nhân**: Lỗi server (exception)  
**Cách sửa**: Xem Visual Studio output window

---

## 📁 CÁC FILE ĐÃ THÊM/SỬA

### File Mới Tạo:
- `src/SixOsTL.Application/DTOs/TaiLieu/LichSuXemVideoDto.cs`
- `src/SixOsTL.MVC/wwwroot/dist/js/CommentImageHandler.js`
- `src/SixOsTL.MVC/Views/TaiLieu/LichSuXemVideo.cshtml`
- `src/SixOsTL.MVC/wwwroot/dist/js/TaiLieu/VideoWatcher.js`

### File Sửa Đổi:
- `src/SixOsTL.Domain/Entities/TaiLieuHoiDap.cs` → Thêm `DuongDanAnhs`
- `src/SixOsTL.Application/DTOs/TaiLieu/HoiDapDto.cs` → Thêm `DanhSachAnhs`
- `src/SixOsTL.Application/DTOs/TaiLieu/CreateHoiDapDto.cs` → Thêm `DanhSachAnhs`
- `src/SixOsTL.Application/Common/Interfaces/ITaiLieuService.cs` → Thêm method
- `src/SixOsTL.Infrastructure/Services/TaiLieuService.cs` → Thêm logic upload
- `src/SixOsTL.MVC/Controllers/TaiLieuController.cs` → Thêm GuiCauHoi xử lý ảnh
- `src/SixOsTL.MVC/Views/Shared/_AppLayout.cshtml` → Thêm nút photo
- `src/SixOsTL.MVC/wwwroot/dist/css/AppLayout.css` → Thêm CSS cho nút
- `src/SixOsTL.MVC/wwwroot/dist/js/AppLayout.js` → Cập nhật submitComment

---

## ✅ TÓNG KẾT

| Bước | Hành động | Thời gian |
|------|----------|---------|
| 1 | Dừng ứng dụng | 5-10s |
| 2 | Build | 15-30s |
| 3 | Migration DB | 5-10s |
| 4 | Chạy ứng dụng | 10-15s |
| 5 | Test gửi ảnh | 2-3s |
| **Tổng** | | **40-70s** |

---

## 🆘 NẾU VẪN KHÔNG ĐƯỢC

Hãy:
1. **Screenshot** lỗi (Console tab)
2. **Paste** Network response
3. **Báo** cho tôi cụ thể

Tôi sẽ giúp debug! 🔧

---

**Status**: ✅ Code đã sẵn sàng, chỉ cần dừng app & build lại!
