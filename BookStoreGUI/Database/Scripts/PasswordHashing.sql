-- Add Email if missing
IF COL_LENGTH('dbo.UserData','Email') IS NULL
    ALTER TABLE dbo.UserData ADD Email VARCHAR(50) NULL;
GO

-- Add hashed password columns (nullable so legacy rows still work)
IF COL_LENGTH('dbo.UserData','PasswordHash') IS NULL
BEGIN
    ALTER TABLE dbo.UserData ADD PasswordHash VARBINARY(32) NULL; -- 32 bytes (SHA-256 PBKDF2)
    ALTER TABLE dbo.UserData ADD PasswordSalt VARBINARY(16) NULL; -- 16-byte salt
END
GO

-- Recreate the auto-ID trigger so it also carries Email/PasswordHash/PasswordSalt
IF OBJECT_ID('dbo.trg_UserData_AutoID','TR') IS NOT NULL
    DROP TRIGGER dbo.trg_UserData_AutoID;
GO
CREATE TRIGGER dbo.trg_UserData_AutoID
ON dbo.UserData
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH src AS (
        SELECT  i.UserName, i.[Password], i.[Type], i.Manager, i.FullName,
                i.Email, i.PasswordHash, i.PasswordSalt,
                ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS rn
        FROM inserted i
    )
    INSERT INTO dbo.UserData
        (UserID, UserName, [Password], [Type], Manager, FullName, Email, PasswordHash, PasswordSalt)
    SELECT
        ISNULL((SELECT MAX(UserID) FROM dbo.UserData), 0) + rn,
        UserName, [Password], [Type], Manager, FullName, Email, PasswordHash, PasswordSalt
    FROM src;
END
GO
