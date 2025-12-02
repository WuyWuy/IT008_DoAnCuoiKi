USE master
GO

-- 1. Tạo Database nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'QuanLyCaPhe')
BEGIN
    CREATE DATABASE QuanLyCaPhe;
END
GO

USE QuanLyCaPhe
GO

-- Xóa bảng Users cũ nếu sai cấu trúc (để tạo lại cho đúng)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    DROP TABLE Users;
END
GO

-- 2. Tạo bảng Users (Cấu trúc khớp 100% với UserStore.cs)
CREATE TABLE Users 
(
    -- Các cột khớp với Code C#
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,    -- Sửa từ UserName -> FullName
    Email NVARCHAR(256) NOT NULL UNIQUE,
    Phone NVARCHAR(50) NULL,
    Address NVARCHAR(500) NULL,         -- Sửa từ UAddress -> Address
    Gender NVARCHAR(20) NULL,           -- Sửa từ BIT -> NVARCHAR để chứa "Male"/"Female"
    
    -- Cột bảo mật (Bắt buộc phải có cả 2)
    PasswordHash NVARCHAR(MAX) NOT NULL,
    PasswordSalt NVARCHAR(MAX) NOT NULL, -- Thêm cột này
    
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    -- Các cột mở rộng cho Admin (Cho phép NULL hoặc Default để code C# cũ vẫn chạy được)
    RoleName NVARCHAR(30) DEFAULT 'Staff',
    RoleLevel BIT DEFAULT 1, -- 0: Admin, 1: Staff
    IsActive BIT DEFAULT 1
);
GO

-- 3. Tạo bảng Products
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products 
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProName NVARCHAR(50) NOT NULL,
        Price DECIMAL(18,0) NOT NULL,
        CateId INT
    );
END
GO

-- 4. Tạo bảng Ingredients
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Ingredients')
BEGIN
    CREATE TABLE Ingredients 
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IngName NVARCHAR(50) NOT NULL,
        Unit NVARCHAR(15) NOT NULL,
        Quantity FLOAT DEFAULT 0
    );
END
GO 

-- 5. Tạo bảng Recipes
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Recipes')
BEGIN
    CREATE TABLE Recipes
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProId INT REFERENCES Products(Id) ON DELETE CASCADE, 
        IngId INT REFERENCES Ingredients(Id) ON DELETE CASCADE, 
        Amount FLOAT NOT NULL
    );
END
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
    N'Tao',                   -- FullName (txtFullName)
    'admin@test.com',                      -- Email (txtEmail)
    '0909000111',                             -- Phone (txtPhone)
    N'123 Đường Code Dạo, TP.HCM',            -- Address (txtAddress)
    'Male',                                   -- Gender (rbMale/rbFemale)
    
    -- Cặp Hash/Salt này tương ứng với mật khẩu: 123456
    'VyoImmrYWWNWEhjr12JzlkapVg9ZjAjz44BiTHA3fts=', 
    '29+A/ET2QovKKX0dh92gDQ==',
    
    'Staff',            -- Mặc định là Nhân viên
    1,                  -- Level 1
    1                   -- Đang hoạt động
);

SELECT * FROM Users;