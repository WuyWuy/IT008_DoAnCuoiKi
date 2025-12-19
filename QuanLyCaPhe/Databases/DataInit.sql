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
    HourlyWage DECIMAL(18,2) DEFAULT 0,

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

-- =============================================
-- KHU VỰC 3: GIAO DỊCH (TRANSACTION)
-- =============================================

-- Bảng 6: Bills (Hóa đơn)
CREATE TABLE Bills 
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DateCheckIn DATETIME2 DEFAULT SYSUTCDATETIME(),
    DateCheckOut DATETIME2 NULL,
    Status INT DEFAULT 0,              -- 0: Chưa thanh toán, 1: Đã thanh toán
    Discount INT DEFAULT 0,            -- Giảm giá %
    TotalPrice DECIMAL(18,0) DEFAULT 0,
    
    TableId INT NOT NULL,
    UserId INT NULL,                   -- Nhân viên thanh toán
    
    CONSTRAINT FK_Bills_TableCoffees FOREIGN KEY (TableId) REFERENCES TableCoffees(Id),
    CONSTRAINT FK_Bills_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

-- Bảng 7: BillInfos (Chi tiết hóa đơn)
CREATE TABLE BillInfos 
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BillId INT NOT NULL,
    ProId INT NOT NULL,
    Count INT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_BillInfos_Bills FOREIGN KEY (BillId) REFERENCES Bills(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BillInfos_Products FOREIGN KEY (ProId) REFERENCES Products(Id)
);
GO

-- Bảng 8: WorkSchedules (Lịch làm việc)
CREATE TABLE WorkSchedules 
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    WorkDate DATE NOT NULL,
    StartTime TIME(7) NOT NULL,
    EndTime TIME(7) NOT NULL,
    Notes NVARCHAR(500) NULL,
    
    CONSTRAINT FK_WorkSchedules_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

-- Bảng 9: InputInfos (Lịch sử nhập kho)
CREATE TABLE InputInfos 
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IngId INT NOT NULL,
    DateInput DATETIME2 DEFAULT SYSUTCDATETIME(),
    InputPrice DECIMAL(18,0) NOT NULL,
    Count FLOAT NOT NULL,
    
    CONSTRAINT FK_InputInfos_Ingredients FOREIGN KEY (IngId) REFERENCES Ingredients(Id) ON DELETE CASCADE
);
GO

-- Bảng 10: Activities
CREATE TABLE Activities (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ActivityType NVARCHAR(50), -- 'Order', 'Payment', 'Warning'
    Description NVARCHAR(200), -- VD: "Thanh toán: Bàn 01"
    Detail NVARCHAR(500),      -- VD: "Tổng: 45.000 VNĐ"
    TimeAgo DATETIME DEFAULT GETDATE()
)
GO

-- =============================================
-- KHU VỰC 4: DỮ LIỆU MẪU (SEED DATA)
-- =============================================

-- 1. Thêm Admin mặc định (Pass: 123456)
INSERT INTO Users (FullName, Email, Phone, Address, Gender, PasswordHash, PasswordSalt, RoleName, RoleLevel, IsActive)
VALUES 
(
    N'Admin System',
    'admin@test.com',
    '0909000111',
    N'123 Đường Code Dạo, TP.HCM',
    N'Nữ',
    'VyoImmrYWWNWEhjr12JzlkapVg9ZjAjz44BiTHA3fts=', 
    '29+A/ET2QovKKX0dh92gDQ==',
    'Admin',
    0, 
    1
);

-- 2. Thêm Bàn
INSERT INTO TableCoffees (TableName) VALUES 
(N'Bàn 01'), (N'Bàn 02'), (N'Bàn 03'), (N'Bàn 04'), (N'Bàn 05'),
(N'Bàn 06'), (N'Bàn 07'), (N'Bàn 08'), (N'Bàn 09'), (N'Bàn 10');

-- 3. Thêm Món
INSERT INTO Products (ProName, Price) VALUES 
(N'Cà Phê Đen', 15000),
(N'Cà Phê Sữa', 20000),
(N'Bạc Xỉu', 25000),
(N'Trà Đào Cam Sả', 35000),
(N'Sinh Tố Bơ', 40000),
(N'Nước Cam', 25000),
(N'Nước Chanh', 25000),
(N'Matcha Đá Xay', 45000),
(N'Cokkie Đá Xay', 45000);

GO

-- 3a. Thêm Nguyên liệu (seed) - Ingredients needed for products
INSERT INTO Ingredients (IngName, Unit, Quantity) VALUES
(N'Hạt cà phê', N'g',10000), --1
(N'Sữa', N'ml',10000), --2
(N'Sữa đặc', N'ml',5000), --3
(N'Hỗn hợp trà đào', N'g',2000), --4
(N'Bơ', N'g',5000), --5
(N'Nước cam', N'ml',5000), --6
(N'Nước chanh', N'ml',3000), --7
(N'Bột matcha', N'g',2000), --8
(N'Bột cookie', N'g',2000), --9
(N'Đá bào', N'g',20000); --10
GO

-- 3b. Thêm Công thức (Recipe) cho từng món (ProId theo thứ tự insert ở trên)
-- Product IDs:1..9 correspond to the Products inserted earlier
INSERT INTO Recipes (ProId, IngId, Amount) VALUES
-- Cà Phê Đen (ProId =1)
(1,1,10.0),
-- Cà Phê Sữa (ProId =2)
(2,1,10.0),
(2,2,50.0),
-- Bạc Xỉu (ProId =3)
(3,1,8.0),
(3,3,40.0),
-- Trà Đào Cam Sả (ProId =4)
(4,4,15.0),
(4,6,30.0),
-- Sinh Tố Bơ (ProId =5)
(5,5,150.0),
(5,2,50.0),
(5,10,100.0),
-- Nước Cam (ProId =6)
(6,6,200.0),
(6,10,100.0),
-- Nước Chanh (ProId =7)
(7,7,50.0),
(7,10,100.0),
-- Matcha Đá Xay (ProId =8)
(8,8,10.0),
(8,2,150.0),
(8,10,150.0),
-- Cokkie Đá Xay (ProId =9)
(9,9,30.0),
(9,2,150.0),
(9,10,150.0);
GO

USE QuanLyCaPhe
GO

-- KHAI BÁO BIẾN ĐỂ LƯU ID HÓA ĐƠN VỪA TẠO
DECLARE @NewBillId INT;

-- 1. TẠO HÓA ĐƠN (BILLS)
-- Giả sử: Bàn ID 1, Nhân viên ID 1, Giảm giá 10%, Trạng thái 1 (Đã thanh toán)
INSERT INTO Bills (DateCheckIn, DateCheckOut, Status, Discount, TotalPrice, TableId, UserId)
VALUES (
    GETDATE(),              -- Giờ vào: Bây giờ
    DATEADD(minute, 45, GETDATE()), -- Giờ ra: 45 phút sau
    1,                      -- Status: 1 (Đã thanh toán)
    10,                     -- Discount: 10%
    0,                      -- TotalPrice: Tạm để 0, lát tính sau
    1,                      -- TableId: 1
    1                       -- UserId: 1
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
SELECT * FROM Activities