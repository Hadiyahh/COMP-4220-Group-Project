-- Category
CREATE TABLE [dbo].[Category] (
    [CategoryID]  INT           NOT NULL,
    [Name]        VARCHAR (80)  NULL,
    [Description] VARCHAR (255) NULL,
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([CategoryID] ASC)
);
