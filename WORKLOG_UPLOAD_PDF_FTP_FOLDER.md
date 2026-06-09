# Nhật ký sửa chức năng lưu tài liệu PDF chọn thư mục FTP

- Thời gian: 2026-06-09 09:26:10 +07:00
- Repo: D:\project\SixOsWebTL
- Yêu cầu: Khi lưu/upload tài liệu PDF phải cho người dùng chọn thư mục trong FTP trước, sau đó upload PDF vào thư mục đã chọn và lưu đường dẫn file FTP vào DB.

## Các bước đã làm

1. Khảo sát luồng hiện tại
   - Kiểm tra AdminController.UploadFile: trước đó tự lưu vào iles/cn_{idChucNang}/{ten-file}.pdf.
   - Kiểm tra TaiLieuService.CreateFileAsync: đã lưu DuongDanFile vào bảng tài liệu (TaiLieuFile.DuongDanFile).
   - Kiểm tra module video: đã có sẵn API GetFtpFolders và UI chọn folder FTP, có thể tái sử dụng cho tài liệu PDF.

2. Sửa backend upload tài liệu
   - File: src/SixOsTL.MVC/Controllers/AdminController.cs
   - Thêm tham số emoteFolder cho UploadFile.
   - Bắt buộc chọn thư mục FTP trước khi upload; nếu chưa chọn thì trả lỗi Chưa chọn thư mục FTP để lưu tài liệu.
   - Chặn upload không phải PDF bằng kiểm tra extension .pdf.
   - Upload vào {remoteFolder}/{slug}.pdf.
   - Dùng esult.RemotePath để gọi CreateFileAsync, nên DB tiếp tục lưu đúng đường dẫn FTP thực tế vào DuongDanFile.

3. Sửa giao diện quản lý tài liệu
   - File: src/SixOsTL.MVC/Views/Admin/QuanLyFile.cshtml
   - Thêm ô emoteFolder readonly + nút Chọn trong modal Upload Tài liệu.
   - Thêm modal duyệt/chọn thư mục FTP: Root, Lên, breadcrumb, danh sách folder con, chọn folder hiện tại.

4. Sửa JavaScript quản lý tài liệu
   - File: src/SixOsTL.MVC/wwwroot/dist/js/admin/quanly/QuanLyFile.js
   - Tái sử dụng logic chọn folder FTP từ QuanLyVideo.js.
   - openFolderPicker() gọi /Admin/GetFtpFolders để tải thư mục FTP.
   - Khi chọn folder, giá trị được ghi vào input emoteFolder; submitUpload sẽ gửi kèm về backend.

## Lỗi gặp phải và cách xử lý

1. Build solution trực tiếp bị lỗi do file DLL đang bị khóa
   - Lệnh: dotnet build SixOsTL.sln
   - Lỗi chính: MSB3027/MSB3021 do tiến trình SixOsTL.MVC (23088) và Visual Studio đang giữ các DLL trong src/SixOsTL.MVC/bin/Debug/net9.0.
   - Cách xử lý: build kiểm tra ra thư mục tạm để tránh ghi đè DLL đang chạy.
   - Lệnh thay thế: dotnet build src\SixOsTL.MVC\SixOsTL.MVC.csproj -o .\artifacts\verify-build

## Kết quả kiểm tra

- Build kiểm tra thành công với lệnh:
  dotnet build src\SixOsTL.MVC\SixOsTL.MVC.csproj -o .\artifacts\verify-build
- Kết quả: Build succeeded, 0 error.
- Còn 6 warning nullable/limited stream có sẵn, không phát sinh lỗi compile từ thay đổi upload PDF.

## Ghi chú vận hành

- Muốn build vào output Debug mặc định thì cần dừng app SixOsTL.MVC đang chạy hoặc dừng debug IIS/Kestrel trong Visual Studio trước.
- Luồng mới: Admin mở Quản lý Tài liệu -> Upload Tài liệu mới -> chọn chức năng/STT/tên/keyword -> bấm Chọn thư mục FTP -> chọn folder -> chọn PDF -> Upload.
- Đường dẫn lưu DB là đường dẫn FTP trả về từ _ftp.UploadAsync, được ghi vào TaiLieuFile.DuongDanFile.

## Cập nhật sửa lỗi font chữ

- Thời gian: 2026-06-09 09:34:12 +07:00
- Lỗi: Sau lần sửa đầu, một số text tiếng Việt mới thêm trong AdminController.cs và QuanLyFile.cshtml bị mojibake dạng Ch?a ch?n, Th? m?c, ??ng do ghi file với encoding không khớp.
- Cách sửa:
  - Sửa lại các message backend trong UploadFile: Chưa chọn file, Chưa chọn thư mục FTP để lưu tài liệu, Chỉ cho phép upload file PDF.
  - Sửa lại label/nút/modal trong QuanLyFile.cshtml: Thư mục lưu tài liệu, Chưa chọn folder FTP, Chọn, Chọn thư mục FTP, Lên, Đang tải danh sách folder, Chọn folder này, Đóng.
- Kiểm tra lại: dotnet build src\SixOsTL.MVC\SixOsTL.MVC.csproj -o .\artifacts\verify-build thành công, 0 error.

## Cập nhật sửa triệt để lỗi font trong modal FTP

- Thời gian: 2026-06-09 09:44:01 +07:00
- Lỗi: Trình duyệt vẫn hiển thị các text mới thêm dạng Ch?n th? m?c FTP, Ch?n folder n?y, ??ng.
- Cách sửa: Chuyển các text mới thêm trong Views/Admin/QuanLyFile.cshtml sang HTML entities (Ch&#7885;n, Th&#432; m&#7909;c, &#272;&#243;ng, ...), và chuyển text động trong QuanLyFile.js sang Unicode escape để không phụ thuộc encoding file/browser.
- Kiểm tra: dotnet build src\SixOsTL.MVC\SixOsTL.MVC.csproj -o .\artifacts\verify-build thành công, 0 error.
- Lưu ý: nếu trình duyệt vẫn hiện bản cũ thì cần hard refresh (Ctrl+F5) hoặc restart app do sp-append-version chỉ cập nhật sau khi server nhận file mới.
