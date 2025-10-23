-- Orders (depends on UserData)
CREATE TABLE [dbo].[Orders] (
    [OrderID]         INT      IDENTITY (1, 1) NOT NULL,
    [UserID]          INT      NOT NULL,
    [OrderDate]       DATETIME CONSTRAINT [DF_Orders_OrderDate] DEFAULT (getdate()) NOT NULL,
    [Status]          CHAR (1) CONSTRAINT [DF_Orders_StatusCode] DEFAULT ('P') NOT NULL,
    [DiscountPercent] INT      DEFAULT ((0)) NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([OrderID] ASC),
    CONSTRAINT [FK_Orders_Employee] FOREIGN KEY ([UserID]) REFERENCES [dbo].[UserData] ([UserID])
);
