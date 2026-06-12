-- Script to consolidate "Khám bệnh" and "Đăng nhập PM" under the same product
-- PHẦN MỀM SIXOS VERSION 1.1

-- Step 1: Check current structure
SELECT 'Current Structure:' as Info;

SELECT
    sp.Id as ProductID,
    sp.TenSanPham as ProductName,
    cn.Id as FunctionID,
    cn.ChucNang as FunctionName
FROM DM_SanPham sp
    LEFT JOIN DM_ChucNang cn ON sp.Id = cn.IDSanPham
WHERE
    sp.IsDeleted = 0
ORDER BY sp.Id, cn.ChucNang;

-- Step 2: Find the product ID for "PHẦN MỀM SIXOS VERSION 1.1"
DECLARE @ProductID BIGINT = (
    SELECT Id
    FROM DM_SanPham
    WHERE
        TenSanPham LIKE N'%PHẦN MỀM SIXOS%VERSION 1.1%'
        AND IsDeleted = 0
);

-- Step 3: Update "Khám bệnh" function to this product
UPDATE DM_ChucNang
SET
    IDSanPham = @ProductID
WHERE
    ChucNang = N'Khám bệnh'
    AND IsDeleted = 0;

-- Step 4: Update "Đăng nhập PM" function to this product
UPDATE DM_ChucNang
SET
    IDSanPham = @ProductID
WHERE
    ChucNang = N'Đăng nhập PM'
    AND IsDeleted = 0;

-- Step 5: Verify the changes
SELECT 'After Consolidation:' as Info;

SELECT
    sp.Id as ProductID,
    sp.TenSanPham as ProductName,
    cn.Id as FunctionID,
    cn.ChucNang as FunctionName
FROM DM_SanPham sp
    LEFT JOIN DM_ChucNang cn ON sp.Id = cn.IDSanPham
WHERE
    sp.IsDeleted = 0
    AND (
        cn.ChucNang = N'Khám bệnh'
        OR cn.ChucNang = N'Đăng nhập PM'
        OR sp.Id = @ProductID
    )
ORDER BY sp.Id, cn.ChucNang;