-- Create the database (if not exists)
CREATE DATABASE FireTrackDB;
GO
USE FireTrackDB;
GO

-- 1. USERS TABLE
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) NOT NULL, -- 'Admin', 'User', 'Chief'
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 2. EQUIPMENT / INVENTORY TABLE
CREATE TABLE Equipment (
    EquipmentId INT PRIMARY KEY IDENTITY(1,1),
    EquipmentName NVARCHAR(100) NOT NULL,
    SerialNumber NVARCHAR(50) UNIQUE NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Available', -- 'Available', 'Assigned', 'Maintenance'
    Location NVARCHAR(100),
    QRCodeContent NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 3. REQUESTS TABLE
CREATE TABLE Requests (
    RequestId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    EquipmentId INT FOREIGN KEY REFERENCES Equipment(EquipmentId),
    RequestDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Approved', 'Denied'
    Notes NVARCHAR(MAX)
);

-- 4. TRANSFERS TABLE
CREATE TABLE Transfers (
    TransferId INT PRIMARY KEY IDENTITY(1,1),
    FromUserId INT FOREIGN KEY REFERENCES Users(UserId),
    ToUserId INT FOREIGN KEY REFERENCES Users(UserId),
    EquipmentId INT FOREIGN KEY REFERENCES Equipment(EquipmentId),
    TransferDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending'
);

-- 5. CLEARANCES TABLE
CREATE TABLE Clearances (
    ClearanceId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    ClearanceDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending'
);

-- 6. NOTIFICATIONS TABLE (This is the empty one you are seeing)
CREATE TABLE Notifications (
    NotificationId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    Message NVARCHAR(255) NOT NULL,
    NotificationType NVARCHAR(50), -- 'Request', 'Transfer', 'System'
    IsRead BIT DEFAULT 0, -- 0 = Unread, 1 = Read
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Add indexes to speed up notification fetching
CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);


-- Insert a test Admin User
INSERT INTO Users (FullName, Role, Email, PasswordHash) 
VALUES ('Admin Chief', 'Admin', 'admin@firetrack.com', 'hashedpassword123');

-- Insert a test piece of equipment
INSERT INTO Equipment (EquipmentName, SerialNumber, Status, Location, QRCodeContent) 
VALUES ('Fire Hose MK-II', 'FH-2026-001', 'Available', 'Station A', 'QR-FH-001');

-- !!! CRITICAL DEBUG STEP: Insert a specific Notification for Admin
-- This is likely why your app shows empty. Ensure UserId matches the Admin ID (probably '1')
INSERT INTO Notifications (UserId, Message, NotificationType, IsRead) 
VALUES (1, 'New equipment request pending review.', 'Request', 0);

INSERT INTO Notifications (UserId, Message, NotificationType, IsRead) 
VALUES (1, 'User Transfer completed for Hose MK-II.', 'Transfer', 0);





-- Check if the current Admin actually has notifications in the database
SELECT * FROM Notifications 
WHERE UserId = 1 
ORDER BY CreatedAt DESC;

-- If the above returns 0 rows, your backend is looking for the wrong user ID
-- To check which User IDs actually exist:
SELECT UserId, FullName, Role FROM Users;