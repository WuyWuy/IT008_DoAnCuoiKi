USE master
GO

-- 1. XÓA DATABASE CŨ NẾU CÓ (Để làm lại từ đầu)
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'QuanLyCaPhe')
BEGIN
    ALTER DATABASE QuanLyCaPhe SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyCaPhe;
END
GO

-- 2. TẠO DATABASE MỚI
CREATE DATABASE QuanLyCaPhe;
GO

USE QuanLyCaPhe
GO

-- =============================================
-- KHU VỰC 1: CÁC BẢNG QUẢN TRỊ (SYSTEM)
-- =============================================

-- Bảng 1: Users (Nhân viên)
CREATE TABLE Users 
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    Phone NVARCHAR(50) NULL,
    Address NVARCHAR(500) NULL,
    Gender NVARCHAR(20) NULL,           
    
    -- Bảo mật
    PasswordHash NVARCHAR(MAX) NOT NULL,
    PasswordSalt NVARCHAR(MAX) NOT NULL,
    
    -- Phân quyền & Trạng thái
    RoleName NVARCHAR(30) DEFAULT 'Staff', -- 'Admin', 'Staff'
    RoleLevel BIT DEFAULT 1,               -- 0: Admin, 1: Staff
    IsActive BIT DEFAULT 1,                -- 1: Active, 0: Locked
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Bảng 2: TableCoffees (Danh sách bàn)
CREATE TABLE TableCoffees 
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TableName NVARCHAR(50) NOT NULL,
    Status NVARCHAR(20) DEFAULT N'Trống'   -- 'Trống', 'Có người'
);
GO

-- Bảng 3: Ingredients (Nguyên liệu kho)
CREATE TABLE Ingredients 
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IngName NVARCHAR(100) NOT NULL,
    Unit NVARCHAR(20) NOT NULL,        -- kg, lít, hộp...
    Quantity FLOAT DEFAULT 0           -- Tồn kho hiện tại
);
GO

-- =============================================
-- KHU VỰC 2: SẢN PHẨM & CÔNG THỨC (ĐÃ BỎ CATEGORY)
-- =============================================

-- Bảng 4: Products (Món ăn/Đồ uống) -> ĐÃ BỎ CỘT CateId
CREATE TABLE Products 
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProName NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,0) NOT NULL
);
GO

-- Bảng 5: Recipes (Công thức pha chế)
CREATE TABLE Recipes
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProId INT NOT NULL,
    IngId INT NOT NULL,
    Amount FLOAT NOT NULL, 
    
    CONSTRAINT FK_Recipes_Products FOREIGN KEY (ProId) REFERENCES Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Recipes_Ingredients FOREIGN KEY (IngId) REFERENCES Ingredients(Id) ON DELETE CASCADE
);
GO

USE QuanLyCaPhe
GO

INSERT INTO Users 
(
    -- Các cột lấy từ giao diện Register (XAML)
    FullName, 
    Email, 
    Phone, 
    Address, 
    Gender, 
    
    -- Các cột bảo mật (Hash của mật khẩu "123456")
    PasswordHash, 
    PasswordSalt,
    
    -- Các cột mặc định (Code C# Register chưa xử lý nên ta điền mặc định)
    RoleName, 
    RoleLevel, 
    IsActive
)
VALUES 
(
    N'Admin System',
    'admin@test.com',
    '0909000111',
    N'123 Đường Code Dạo, TP.HCM',
    N'Nữ',
    'VyoImmrYWWNWEhjr12JzlkapVg9ZjAjz44BiTHA3fts=', 
    '29+A/ET2QovKKX0dh92gDQ==',
    
    'Staff',            -- Mặc định là Nhân viên
    1,                  -- Level 1
    1                   -- Đang hoạt động
);

-- Lấy ID của hóa đơn vừa insert
SET @NewBillId = SCOPE_IDENTITY();

-- 2. THÊM MÓN ĂN VÀO CHI TIẾT (BILLINFOS)
-- Món 1: ID 1 (Cà phê đen), Số lượng: 2 ly
INSERT INTO BillInfos (BillId, ProId, Count) VALUES (@NewBillId, 1, 2);

-- Món 2: ID 3 (Bạc xỉu), Số lượng: 1 ly
INSERT INTO BillInfos (BillId, ProId, Count) VALUES (@NewBillId, 3, 1);

-- 3. CẬP NHẬT LẠI TỔNG TIỀN (TOTALPRICE) CHO BILL
-- Logic: (Tổng tiền món) - (Giảm giá %)
UPDATE Bills
SET TotalPrice = (
    SELECT SUM(p.Price * bi.Count) 
    FROM BillInfos bi
    JOIN Products p ON bi.ProId = p.Id
    WHERE bi.BillId = @NewBillId
) * (100 - Discount) / 100
WHERE Id = @NewBillId;

-- 4. HIỂN THỊ KẾT QUẢ ĐỂ KIỂM TRA
SELECT * FROM Bills WHERE Id = @NewBillId;
SELECT * FROM BillInfos WHERE BillId = @NewBillId;



SELECT * FROM Users
SELECT * FROM Ingredients
