-- OrderItem (depends on Orders & BookData)
CREATE TABLE [dbo].[OrderItem] (
    [OrderID]  INT       NOT NULL,
    [ISBN]     CHAR (10) NOT NULL,
    [Quantity] INT       NOT NULL,
    CONSTRAINT [PK_OrderItem] PRIMARY KEY CLUSTERED ([OrderID] ASC, [ISBN] ASC),
    CONSTRAINT [FK_OrderItem_Orders]  FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_OrderItem_Product] FOREIGN KEY ([ISBN])    REFERENCES [dbo].[BookData] ([ISBN])
);
