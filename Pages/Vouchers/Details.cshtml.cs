using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MiniAccountManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class DetailsModel : PageModel
{
    private readonly IConfiguration _configuration;

    public DetailsModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public VoucherEntryViewModel Voucher { get; set; }
    public List<VoucherDetailViewModel> Entries { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        // Get master
        using (var cmd = new SqlCommand("SELECT * FROM VoucherMaster WHERE Id = @Id", conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                Voucher = new VoucherEntryViewModel
                {
                    ReferenceNo = reader["ReferenceNo"].ToString(),
                    VoucherType = reader["VoucherType"].ToString(),
                    VoucherDate = Convert.ToDateTime(reader["VoucherDate"]),
                    CreatedBy = reader["CreatedBy"].ToString()
                };
            }
        }

        // Get details
        using (var cmd = new SqlCommand(@"
            SELECT vd.*, coa.AccountName 
            FROM VoucherDetails vd 
            JOIN ChartOfAccounts coa ON coa.Id = vd.AccountId 
            WHERE vd.VoucherId = @VoucherId", conn))
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
}
