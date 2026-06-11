-- ==============================================================
-- SCRIPT NHANH: Thêm quyền ADMIN cho tài khoản
-- ==============================================================
-- HƯỚNG DẪN: 
-- 1. Chạy BƯỚC 1 trước để xem danh sách tài khoản
-- 2. Tìm tên tài khoản của bạn
-- 3. Thay 'TEN_TAI_KHOAN_CUA_BAN' ở BƯỚC 2 bằng tên thật
-- 4. Chạy BƯỚC 2
-- ==============================================================

-- ============== BƯỚC 1: XEM DANH SÁCH TÀI KHOẢN ==============
SELECT Id, TenTK, HoTen, Active 
FROM TaiKhoanDaoTao 
WHERE IsDeleted = 0
ORDER BY Id;

-- ============== BƯỚC 2: THÊM QUYỀN ADMIN ==============
-- >>> THAY 'TEN_TAI_KHOAN_CUA_BAN' BẰNG TÊN TÀI KHOẢN BẠN DÙNG ĐỂ LOGIN <<<

-- Tạo vai trò ADMIN nếu chưa có
IF NOT EXISTS (SELECT 1 FROM DmVaiTro WHERE MaVaiTro = 'ADMIN')
    INSERT INTO DmVaiTro (MaVaiTro, TenVaiTro, MoTa, Active)
    VALUES ('ADMIN', N'Quản trị viên', N'Toàn quyền hệ thống', 1);

-- Gán quyền ADMIN cho tài khoản
DECLARE @TenTK NVARCHAR(100) = 'TEN_TAI_KHOAN_CUA_BAN';  -- <<< SỬA DÒNG NÀY

DECLARE @IdTK BIGINT = (SELECT Id FROM TaiKhoanDaoTao WHERE TenTK = @TenTK AND IsDeleted = 0);
DECLARE @IdVT BIGINT = (SELECT Id FROM DmVaiTro WHERE MaVaiTro = 'ADMIN');

IF @IdTK IS NULL
    PRINT N'❌ Không tìm thấy tài khoản!';
ELSE IF EXISTS (SELECT 1 FROM TaiKhoanVaiTro WHERE IDTaiKhoan = @IdTK AND IDVaiTro = @IdVT)
    PRINT N'✅ Tài khoản đã có quyền ADMIN rồi';
ELSE
BEGIN
    INSERT INTO TaiKhoanVaiTro (IDTaiKhoan, IDVaiTro) VALUES (@IdTK, @IdVT);
    PRINT N'✅ Đã thêm quyền ADMIN thành công!';
END

-- ============== BƯỚC 3: KIỂM TRA KẾT QUẢ ==============
SELECT 
    tk.Id,
    tk.TenTK,
    tk.HoTen,
    vt.MaVaiTro,
    vt.TenVaiTro
FROM TaiKhoanDaoTao tk
INNER JOIN TaiKhoanVaiTro tv ON tk.Id = tv.IDTaiKhoan
INNER JOIN DmVaiTro vt ON tv.IDVaiTro = vt.Id
WHERE tk.TenTK = 'TEN_TAI_KHOAN_CUA_BAN'  -- <<< SỬA DÒNG NÀY GIỐNG TRÊN
  AND tk.IsDeleted = 0;
