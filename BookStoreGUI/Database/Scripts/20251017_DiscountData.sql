-- DiscountData (independent)
CREATE TABLE [dbo].[DiscountData] (
    [Ccode]        VARCHAR (10)    NOT NULL,
    [discount]     DECIMAL (18, 2) NOT NULL,
    [DiscountDesc] VARCHAR (50)    NULL,
    PRIMARY KEY CLUSTERED ([Ccode] ASC)
);
