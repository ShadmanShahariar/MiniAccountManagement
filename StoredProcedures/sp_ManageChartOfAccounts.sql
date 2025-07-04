USE [AccountManagementDB]
GO
/****** Object:  StoredProcedure [dbo].[sp_ManageChartOfAccounts]    Script Date: 7/2/2025 10:48:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:     Shadmun Shahariar
-- Create date: 2 Jul 2025
-- Description: sp_ManageChartOfAccounts
-- =============================================
ALTER PROCEDURE [dbo].[sp_ManageChartOfAccounts]
    @Option NVARCHAR(10),
    @Id INT = NULL,
    @AccountName NVARCHAR(100) = NULL,
    @ParentId INT = NULL,
    @AccountType NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Option = 'INSERT'
    BEGIN
        INSERT INTO ChartOfAccounts (AccountName, ParentId, AccountType)
        VALUES (@AccountName, @ParentId, @AccountType)

        SELECT * FROM ChartOfAccounts WHERE IsActive = 1;
    END
    ELSE IF @Option = 'UPDATE'
    BEGIN
        UPDATE ChartOfAccounts
        SET AccountName = @AccountName, ParentId = @ParentId, AccountType = @AccountType
        WHERE Id = @Id

        SELECT * FROM ChartOfAccounts WHERE IsActive = 1;
    END
    ELSE IF @Option = 'DELETE'
    BEGIN
        UPDATE ChartOfAccounts
        SET IsActive = 0
        WHERE Id = @Id

        SELECT * FROM ChartOfAccounts WHERE IsActive = 1;
    END
    ELSE IF @Option = 'GET'
    BEGIN
        SELECT * FROM ChartOfAccounts WHERE IsActive = 1;
    END
END
