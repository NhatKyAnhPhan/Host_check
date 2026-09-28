/* =========================================================
   RESET DATABASE
========================================================= */
IF DB_ID('QL_HOST') IS NOT NULL
BEGIN
    ALTER DATABASE QL_HOST SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QL_HOST;
END
GO

CREATE DATABASE QL_HOST
GO
USE QL_HOST
GO

/* =========================================================
   DROP TABLES
========================================================= */
IF OBJECT_ID('Event_Logs', 'U') IS NOT NULL DROP TABLE Event_Logs;
IF OBJECT_ID('Alerts', 'U') IS NOT NULL DROP TABLE Alerts;
IF OBJECT_ID('Host_Stats', 'U') IS NOT NULL DROP TABLE Host_Stats;
IF OBJECT_ID('Hosts', 'U') IS NOT NULL DROP TABLE Hosts;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('Roles', 'U') IS NOT NULL DROP TABLE Roles;
GO

/* =========================================================
   ROLES
========================================================= */
CREATE TABLE Roles (
    RoleID INT IDENTITY PRIMARY KEY,
    RoleName VARCHAR(50) UNIQUE NOT NULL,
    Description NVARCHAR(200)
);
GO

INSERT INTO Roles VALUES
('Admin', N'Quản trị hệ thống'),
('User',  N'Người dùng thường');
GO

/* =========================================================
   USERS
========================================================= */
CREATE TABLE Users (
    UserID INT IDENTITY PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName NVARCHAR(100),
    Email VARCHAR(100),
    RoleID INT NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME NULL,

    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

INSERT INTO Users (Username, PasswordHash, FullName, Email, RoleID)
VALUES
(
    'admin',
    CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', '123'), 2),
    N'Quản trị viên',
    'admin@host.local',
    1
),
(
    'user1',
    CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', '123'), 2),
    N'Người dùng 1',
    'user1@host.local',
    2
);
GO
/* =========================================================
   HOSTS
========================================================= */
CREATE TABLE Hosts (
    HostID INT IDENTITY PRIMARY KEY,
    HostName NVARCHAR(100) NOT NULL,
    IPAddress VARCHAR(15) UNIQUE NOT NULL,
    Description NVARCHAR(255),
    OS VARCHAR(50),
    Location NVARCHAR(100),
    Environment VARCHAR(50) CHECK (Environment IN ('Production','Staging','Development')),
    IsActive BIT DEFAULT 1,
    LastSeen DATETIME DEFAULT GETDATE(),
    AddedAt DATETIME DEFAULT GETDATE()
);
GO

INSERT INTO Hosts 
(HostName, IPAddress, Description, OS, Location, Environment, IsActive, LastSeen, AddedAt)
VALUES
(N'LOCALHOST', '127.0.0.1', N'Máy local', 'Windows 10', N'Local', 'Development', 1, GETDATE(), GETDATE()),
(N'PC_LAN', '192.168.1.5', N'Máy trong LAN', 'Windows 11', N'Phòng máy', 'Development', 1, GETDATE(), GETDATE()),
(N'GOOGLE_DNS', '8.8.8.8', N'Google DNS', 'Linux', N'Internet', 'Production', 1, GETDATE(), GETDATE()),
(N'CLOUDFLARE', '1.1.1.1', N'Cloudflare DNS', 'Linux', N'Internet', 'Production', 1, GETDATE(), GETDATE()),
(N'INVALID_TEST', '10.10.10.10', N'Host test offline', 'Unknown', N'Unknown', 'Staging', 1, GETDATE(), GETDATE());
GO

/* =========================================================
   HOST_STATS
========================================================= */
CREATE TABLE Host_Stats (
    StatID INT IDENTITY PRIMARY KEY,
    HostID INT NOT NULL,
    CPU DECIMAL(5,2) CHECK (CPU BETWEEN 0 AND 100),
    RAM DECIMAL(5,2) CHECK (RAM BETWEEN 0 AND 100),
    Disk DECIMAL(5,2) CHECK (Disk BETWEEN 0 AND 100),
    NetworkIn DECIMAL(10,2),
    NetworkOut DECIMAL(10,2),
    RecordedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (HostID) REFERENCES Hosts(HostID)
);
GO

INSERT INTO Host_Stats
(HostID, CPU, RAM, Disk, NetworkIn, NetworkOut, RecordedAt)
VALUES
(1, 25.5, 40.2, 30.1, 120, 80, DATEADD(MINUTE,-1,GETDATE())),
(2, 55.2, 60.1, 50.0, 300, 200, DATEADD(MINUTE,-2,GETDATE())),
(3, 15.0, 30.0, 20.0, 100, 50, DATEADD(MINUTE,-3,GETDATE())),
(4, 10.5, 25.3, 18.0, 90, 40, DATEADD(MINUTE,-4,GETDATE())),
(5, 0.0,  0.0,  0.0,  0,   0,  DATEADD(MINUTE,-5,GETDATE()));
GO


/* =========================================================
   ALERTS
========================================================= */
CREATE TABLE Alerts (
    AlertID INT IDENTITY PRIMARY KEY,
    HostID INT NOT NULL,
    AlertType VARCHAR(50),
    ThresholdValue DECIMAL(5,2),
    Message NVARCHAR(200),
    Level VARCHAR(20) CHECK (Level IN ('info','warning','critical')),
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsRead BIT DEFAULT 0,

    FOREIGN KEY (HostID) REFERENCES Hosts(HostID)
);
GO

INSERT INTO Alerts VALUES
(1,'CPU',90,N'CPU vượt ngưỡng 92.8%','critical',GETDATE(),0),
(2,'NETWORK',NULL,N'Lưu lượng mạng cao','info',GETDATE(),1);
GO

/* =========================================================
   EVENT LOGS
========================================================= */
CREATE TABLE Event_Logs (
    LogID INT IDENTITY PRIMARY KEY,
    HostID INT NOT NULL,
    EventType VARCHAR(50),
    Description NVARCHAR(200),
    UserResponsible NVARCHAR(50),
    LoggedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (HostID) REFERENCES Hosts(HostID)
);
GO

INSERT INTO Event_Logs VALUES
(1,'reboot',N'Restart định kỳ','System',DATEADD(HOUR,-1,GETDATE())),
(2,'config_change',N'Cập nhật Apache','Admin',DATEADD(MINUTE,-30,GETDATE()));
GO

/* =========================================================
   INDEXES
========================================================= */
CREATE INDEX IX_HostStats_Time ON Host_Stats (RecordedAt DESC);
CREATE INDEX IX_Alerts_Read ON Alerts (IsRead);
GO

/* =========================================================
   VIEW LOGIN
========================================================= */
CREATE OR ALTER VIEW VW_UserLogin
AS
SELECT 
    u.UserID,
    u.Username,
    u.PasswordHash,
    u.FullName,
    r.RoleName,
    u.IsActive
FROM Users u
JOIN Roles r ON u.RoleID = r.RoleID;
GO

/* =========================================================
   STORED PROCEDURE LOGIN
========================================================= */
CREATE OR ALTER PROCEDURE SP_Login
    @Username VARCHAR(50),
    @Password VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.UserID,
           u.Username,
           u.FullName,
           r.RoleName
    FROM Users u
    JOIN Roles r ON u.RoleID = r.RoleID
    WHERE u.Username = @Username
      AND u.PasswordHash = CONVERT(VARCHAR(64),
            HASHBYTES('SHA2_256', @Password), 2)
      AND u.IsActive = 1;
END
GO


/* =========================================================
   STORED PROCEDURE REGISTER
========================================================= */
CREATE OR ALTER PROCEDURE SP_Register
    @Username VARCHAR(50),
    @Password VARCHAR(255),
    @FullName NVARCHAR(100),
    @Email VARCHAR(100)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Users WHERE Username=@Username)
    BEGIN
        RAISERROR (N'Tài khoản đã tồn tại',16,1);
        RETURN;
    END

    INSERT INTO Users (Username,PasswordHash,FullName,Email,RoleID)
    VALUES (@Username,@Password,@FullName,@Email,2);
END
GO

/* =========================================================
   TEST
========================================================= */
EXEC SP_Login 'admin','123';
EXEC SP_Register 'testuser','123',N'Test User','test@host.local';
GO
