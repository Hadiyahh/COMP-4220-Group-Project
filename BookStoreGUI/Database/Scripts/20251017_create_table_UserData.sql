-- UserData
CREATE TABLE [dbo].[UserData] (
    [UserID]   INT           NOT NULL,
    [UserName] VARCHAR (20)  NOT NULL,
    [Password] VARCHAR (25)  NOT NULL,
    [Type]     CHAR (2)      NOT NULL,
    [Manager]  BIT           NOT NULL,
    [FullName] NVARCHAR (50) NULL,
    CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED ([UserID] ASC)
);
