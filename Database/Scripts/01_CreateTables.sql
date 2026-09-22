/* =============================================================
   NamSaoHRM – Script tạo cơ sở dữ liệu (tham khảo)
   Lưu ý: Ứng dụng TỰ ĐỘNG tạo CSDL và dữ liệu mẫu ở lần chạy đầu
   (EnsureCreated). Chỉ dùng script này khi muốn tạo CSDL thủ công
   trên một máy chủ khác; khi đó KHÔNG cần chạy ứng dụng tạo lại.
   ============================================================= */
IF DB_ID(N'NamSaoHRM') IS NULL
    CREATE DATABASE NamSaoHRM;
GO
USE NamSaoHRM;
GO

-- 1. Bảng Users: tài khoản đăng nhập
CREATE TABLE Users (
    UserID        INT IDENTITY(1,1) PRIMARY KEY,
    Username      VARCHAR(50)   NOT NULL,
    PasswordHash  VARCHAR(255)  NOT NULL,
    Role          VARCHAR(20)   NOT NULL,
    TrangThai     BIT           NOT NULL DEFAULT 1,
    NgayTao       DATETIME2     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'NhanVien'))
);
GO

-- 2. Bảng PhongBan: phòng ban
CREATE TABLE PhongBan (
    MaPhongBan    INT IDENTITY(1,1) PRIMARY KEY,
    TenPhongBan   NVARCHAR(100) NOT NULL,
    MoTa          NVARCHAR(255) NULL,
    NgayThanhLap  DATE          NULL,
    CONSTRAINT UQ_PhongBan_Ten UNIQUE (TenPhongBan)
);
GO

-- 3. Bảng NhanVien: hồ sơ nhân viên
CREATE TABLE NhanVien (
    MaNV          INT IDENTITY(1,1) PRIMARY KEY,
    UserID        INT           NULL,
    MaPhongBan    INT           NOT NULL,
    HoTen         NVARCHAR(100) NOT NULL,
    NgaySinh      DATE          NOT NULL,
    GioiTinh      NVARCHAR(10)  NOT NULL,
    Email         VARCHAR(100)  NOT NULL,
    SoDienThoai   VARCHAR(15)   NULL,
    ChucVu        NVARCHAR(50)  NULL,
    LuongCoBan    DECIMAL(18,0) NOT NULL,
    NgayVaoLam    DATE          NOT NULL,
    CONSTRAINT UQ_NhanVien_Email UNIQUE (Email),
    CONSTRAINT CK_NhanVien_LuongCoBan CHECK (LuongCoBan > 0),
    CONSTRAINT FK_NhanVien_Users FOREIGN KEY (UserID)
        REFERENCES Users(UserID) ON DELETE SET NULL,
    CONSTRAINT FK_NhanVien_PhongBan FOREIGN KEY (MaPhongBan)
        REFERENCES PhongBan(MaPhongBan)
);
-- Mỗi tài khoản gắn với tối đa 1 nhân viên (bỏ qua giá trị NULL)
CREATE UNIQUE INDEX UX_NhanVien_UserID ON NhanVien (UserID) WHERE UserID IS NOT NULL;
GO

-- 4. Bảng ChamCong: mỗi nhân viên chỉ có 1 bản ghi chấm công mỗi ngày
CREATE TABLE ChamCong (
    MaChamCong    INT IDENTITY(1,1) PRIMARY KEY,
    MaNV          INT           NOT NULL,
    NgayChamCong  DATE          NOT NULL,
    GioCheckIn    DATETIME2     NULL,
    GioCheckOut   DATETIME2     NULL,
    SoGioLam      DECIMAL(4,2)  NULL,
    TrangThai     NVARCHAR(30)  NULL,
    GhiChu        NVARCHAR(255) NULL,
    CONSTRAINT FK_ChamCong_NhanVien FOREIGN KEY (MaNV)
        REFERENCES NhanVien(MaNV) ON DELETE CASCADE,
    CONSTRAINT UQ_ChamCong_NV_Ngay UNIQUE (MaNV, NgayChamCong),
    CONSTRAINT CK_ChamCong_Gio
        CHECK (GioCheckOut IS NULL OR GioCheckOut > GioCheckIn)
);
GO

-- 5. Bảng BangLuong: bảng lương tháng
CREATE TABLE BangLuong (
    MaBangLuong   INT IDENTITY(1,1) PRIMARY KEY,
    MaNV          INT           NOT NULL,
    Thang         INT           NOT NULL,
    Nam           INT           NOT NULL,
    SoNgayCong    DECIMAL(4,1)  NOT NULL,
    SoLanDiMuon   INT           NOT NULL DEFAULT 0,
    PhuCap        DECIMAL(18,0) NOT NULL DEFAULT 0,
    KhauTru       DECIMAL(18,0) NOT NULL DEFAULT 0,
    TongLuong     DECIMAL(18,0) NOT NULL,
    NgayTao       DATETIME2     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_BangLuong_NhanVien FOREIGN KEY (MaNV)
        REFERENCES NhanVien(MaNV) ON DELETE CASCADE,
    CONSTRAINT UQ_BangLuong_NV_Thang UNIQUE (MaNV, Thang, Nam),
    CONSTRAINT CK_BangLuong_Thang CHECK (Thang BETWEEN 1 AND 12)
);
GO
