-- Supplier
CREATE TABLE [dbo].[Supplier] (
    [SupplierId] INT          NOT NULL,
    [Name]       VARCHAR (80) NOT NULL,
    CONSTRAINT [PK_Supplier] PRIMARY KEY CLUSTERED ([SupplierId] ASC)
);
