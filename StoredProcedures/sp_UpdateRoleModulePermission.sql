USE [AccountManagementDB]
GO
/****** Object:  StoredProcedure [dbo].[sp_UpdateRoleModulePermission]    Script Date: 7/2/2025 10:49:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:     Shadmun Shahariar
-- Create date: 2 Jul 2025
-- Description: sp_UpdateRoleModulePermission
-- =============================================
ALTER PROCEDURE [dbo].[sp_UpdateRoleModulePermission]
    @RoleName NVARCHAR(100),
    @ModuleId INT,
    @IsAllowed BIT
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM RoleModulePermissions 
        WHERE RoleName = @RoleName AND ModuleId = @ModuleId
    )
    BEGIN
        UPDATE RoleModulePermissions
        SET IsAllowed = @IsAllowed
        WHERE RoleName = @RoleName AND ModuleId = @ModuleId
    END
    ELSE
    BEGIN
        INSERT INTO RoleModulePermissions (RoleName, ModuleId, IsAllowed)
        VALUES (@RoleName, @ModuleId, @IsAllowed)
    END
END
