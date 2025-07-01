using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MiniAccountManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

public class DetailsModel : PageModel
{
    private readonly IConfiguration _configuration;

    public DetailsModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty]
    public VoucherEntryViewModel Voucher { get; set; } = new();

    [BindProperty]
    public List<VoucherDetailViewModel> Entries { get; set; } = new();

    public SelectList AccountSelectList { get; set; }

    private async Task LoadAccountSelectList(SqlConnection conn)
    {
        var cmd = new SqlCommand("SELECT Id, AccountName FROM ChartOfAccounts WHERE IsDeleted = 0", conn);
        using var reader = await cmd.ExecuteReaderAsync();

        var accounts = new List<SelectListItem>();
        while (await reader.ReadAsync())
        {
            accounts.Add(new SelectListItem
            {
                Value = reader["Id"].ToString(),
                Text = reader["AccountName"].ToString()
            });
        }

        AccountSelectList = new SelectList(accounts, "Value", "Text");
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await LoadAccountSelectList(conn);

        // Get master
        using (var cmd = new SqlCommand("SELECT * FROM VoucherMaster WHERE Id = @Id AND IsDeleted = 0", conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                Voucher = new VoucherEntryViewModel
                {
                    Id = id, // Important for update
                    ReferenceNo = reader["ReferenceNo"].ToString(),
                    VoucherType = reader["VoucherType"].ToString(),
                    VoucherDate = Convert.ToDateTime(reader["VoucherDate"]),
                    CreatedBy = reader["CreatedBy"].ToString()
                };
            }
            else
            {
                return NotFound();
            }
        }

        // Get details
        using (var cmd = new SqlCommand(@"
            SELECT vd.*, coa.AccountName 
            FROM VoucherDetails vd 
            JOIN ChartOfAccounts coa ON coa.Id = vd.AccountId 
            WHERE vd.VoucherId = @VoucherId AND vd.IsDeleted = 0", conn))
        {
            cmd.Parameters.AddWithValue("@VoucherId", id);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Entries.Add(new VoucherDetailViewModel
                {
                    AccountId = Convert.ToInt32(reader["AccountId"]),
                    AccountName = reader["AccountName"].ToString(),
                    Debit = Convert.ToDecimal(reader["DebitAmount"]),
                    Credit = Convert.ToDecimal(reader["CreditAmount"])
                });
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (Voucher == null) Voucher = new VoucherEntryViewModel();

        if (Entries.Sum(x => x.Debit) != Entries.Sum(x => x.Credit))
        {
            ModelState.AddModelError(string.Empty, "Total Debit must equal Total Credit.");
            return Page();
        }

        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        await LoadAccountSelectList(conn);

        using var cmd = new SqlCommand("sp_SaveVoucher", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        var dt = CreateVoucherDetailsDataTable(Entries);

        cmd.Parameters.AddWithValue("@Option", "UPDATE");

        // OUTPUT parameter for @VoucherId
        var voucherIdParam = new SqlParameter("@VoucherId", SqlDbType.Int)
        {
            Direction = ParameterDirection.InputOutput,
            Value = Voucher.Id
        };
        cmd.Parameters.Add(voucherIdParam);

        cmd.Parameters.AddWithValue("@VoucherType", Voucher.VoucherType);
        cmd.Parameters.AddWithValue("@ReferenceNo", Voucher.ReferenceNo);
        cmd.Parameters.AddWithValue("@VoucherDate", Voucher.VoucherDate);
        cmd.Parameters.AddWithValue("@CreatedBy", Voucher.CreatedBy ?? "System");

        var detailsParam = cmd.Parameters.AddWithValue("@VoucherDetails", dt);
        detailsParam.SqlDbType = SqlDbType.Structured;
        detailsParam.TypeName = "dbo.VoucherDetailsType";

        await cmd.ExecuteNonQueryAsync();

        // Update Voucher ID with output (in case it changed)
        Voucher.Id = (int)voucherIdParam.Value;

        return RedirectToPage("./Index");
    }

    private DataTable CreateVoucherDetailsDataTable(List<VoucherDetailViewModel> entries)
    {
        var dt = new DataTable();
        dt.Columns.Add("AccountId", typeof(int));
        dt.Columns.Add("DebitAmount", typeof(decimal));
        dt.Columns.Add("CreditAmount", typeof(decimal));

        foreach (var entry in entries)
        {
            dt.Rows.Add(entry.AccountId, entry.Debit, entry.Credit);
        }

        return dt;
    }
}
