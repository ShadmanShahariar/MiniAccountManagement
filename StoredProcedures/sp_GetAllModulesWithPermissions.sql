USE [AccountManagementDB]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetAllModulesWithPermissions]    Script Date: 7/2/2025 10:48:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:     Shadmun Shahariar
-- Create date: 2 Jul 2025
-- Description: sp_GetAllModulesWithPermissions
-- =============================================
ALTER PROCEDURE [dbo].[sp_GetAllModulesWithPermissions]
    @RoleName NVARCHAR(100)
AS
BEGIN
    SELECT 
        m.Id,
        m.Name,
        ISNULL(rmp.IsAllowed, 0) AS IsAllowed
    FROM Modules m
    LEFT JOIN RoleModulePermissions rmp
        ON m.Id = rmp.ModuleId AND rmp.RoleName = @RoleName;
END
