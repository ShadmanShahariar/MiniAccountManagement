USE [AccountManagementDB]
GO
/****** Object:  StoredProcedure [dbo].[sp_SaveVoucher]    Script Date: 7/2/2025 10:48:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:     Shadmun Shahariar
-- Create date: 2 Jul 2025
-- Description: sp_SaveVoucher
-- =============================================
ALTER   PROCEDURE [dbo].[sp_SaveVoucher]
    @Option NVARCHAR(10),         
    @VoucherId INT OUTPUT,
    @VoucherType NVARCHAR(50),
    @ReferenceNo NVARCHAR(50),
    @VoucherDate DATE,
    @CreatedBy NVARCHAR(50),
    @VoucherDetails dbo.VoucherDetailsType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Option = 'INSERT'
        BEGIN
            INSERT INTO VoucherMaster (VoucherType, ReferenceNo, VoucherDate, CreatedBy, IsDeleted)
            VALUES (@VoucherType, @ReferenceNo, @VoucherDate, @CreatedBy, 0);

            SET @VoucherId = SCOPE_IDENTITY();
        END

        ELSE IF @Option = 'UPDATE'
        BEGIN
            -- Ensure Voucher exists
            IF NOT EXISTS (SELECT 1 FROM VoucherMaster WHERE Id = @VoucherId AND IsDeleted = 0)
            BEGIN
                THROW 50000, 'Voucher not found or already deleted.', 1;
            END

            UPDATE VoucherMaster
            SET VoucherType = @VoucherType,
                ReferenceNo = @ReferenceNo,
                VoucherDate = @VoucherDate
            WHERE Id = @VoucherId;

            -- Soft delete existing details
            UPDATE VoucherDetails
            SET IsDeleted = 1
            WHERE VoucherId = @VoucherId AND IsDeleted = 0;
        END

        ELSE IF @Option = 'DELETE'
        BEGIN
            -- Soft delete voucher master
            UPDATE VoucherMaster
            SET IsDeleted = 1
            WHERE Id = @VoucherId;

            -- Soft delete voucher details
            UPDATE VoucherDetails
            SET IsDeleted = 1
            WHERE VoucherId = @VoucherId;

            COMMIT TRANSACTION;
            RETURN;
        END

        -- Validate @VoucherId before inserting details
        IF @VoucherId IS NULL OR @VoucherId <= 0
        BEGIN
            THROW 50001, 'Invalid VoucherId. Could not insert voucher details.', 1;
        END

        -- Insert new voucher details
        INSERT INTO VoucherDetails (VoucherId, AccountId, DebitAmount, CreditAmount, IsDeleted)
        SELECT @VoucherId, AccountId, DebitAmount, CreditAmount, 0
        FROM @VoucherDetails;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
