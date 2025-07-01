using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountManagement.ViewModels;
using System.Data;

public class EntryModel : PageModel
{
    private readonly IConfiguration _configuration;

    public EntryModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty]
    public VoucherEntryViewModel Voucher { get; set; } = new();

    public List<ChartOfAccountViewModel> Accounts { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAccounts();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        decimal debitTotal = Voucher.Entries.Sum(e => e.Debit);
        decimal creditTotal = Voucher.Entries.Sum(e => e.Credit);

        if (debitTotal != creditTotal)
        {
            ModelState.AddModelError(string.Empty, "Debit and Credit amounts must be equal.");
            await LoadAccounts();
            return Page();
        }
        using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            await conn.OpenAsync();

            var table = new DataTable();
            table.Columns.Add("AccountId", typeof(int));
            table.Columns.Add("DebitAmount", typeof(decimal));
            table.Columns.Add("CreditAmount", typeof(decimal));

            foreach (var entry in Voucher.Entries)
            {
                table.Rows.Add(entry.AccountId, entry.Debit, entry.Credit);
            }

            using (var cmd = new SqlCommand("sp_SaveVoucher", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VoucherType", Voucher.VoucherType);
                cmd.Parameters.AddWithValue("@ReferenceNo", Voucher.ReferenceNo);
                cmd.Parameters.AddWithValue("@VoucherDate", Voucher.VoucherDate);
                cmd.Parameters.AddWithValue("@CreatedBy", User.Identity.Name ?? "System"); // or current user

                var detailParam = cmd.Parameters.AddWithValue("@Details", table);
                detailParam.SqlDbType = SqlDbType.Structured;
                detailParam.TypeName = "TVP_VoucherDetail";

                await cmd.ExecuteNonQueryAsync();
            }
        }

        return RedirectToPage();
    }


    private async Task LoadAccounts()
    {
        Accounts = new List<ChartOfAccountViewModel>();

        using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            await conn.OpenAsync();
            using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Option", "GET");

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Accounts.Add(new ChartOfAccountViewModel
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            AccountName = reader["AccountName"].ToString()
                        });
                    }
                }
            }
        }
    }
}
