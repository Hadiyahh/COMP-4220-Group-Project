-- BookData (depends on Category & Supplier)
CREATE TABLE [dbo].[BookData] (
  [ISBN]       CHAR(10)       NOT NULL,
  [CategoryID] INT            NOT NULL,
  [Title]      VARCHAR(80)    NULL,
  [Author]     VARCHAR(255)   NULL,
  [Price]      DECIMAL(10,2)  NULL,
  [SupplierId] INT            NULL,
  [Year]       NCHAR(4)       NULL,
  [Edition]    NCHAR(2)       NOT NULL,
  [Publisher]  VARCHAR(50)    NULL,
  [InStock]    INT            NOT NULL CONSTRAINT [DF_BookData_InStock] DEFAULT (0),
  CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([ISBN] ASC),
  CONSTRAINT [FK_Product_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Category]([CategoryID]),
  CONSTRAINT [FK_Product_Supplier] FOREIGN KEY ([SupplierId]) REFERENCES [dbo].[Supplier]([SupplierId])
);
