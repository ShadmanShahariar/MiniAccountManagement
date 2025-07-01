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

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAccounts();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validate Debit == Credit
        decimal totalDebit = Voucher.Entries.Sum(e => e.Debit);
        decimal totalCredit = Voucher.Entries.Sum(e => e.Credit);
        if (totalDebit != totalCredit)
        {
            ModelState.AddModelError(string.Empty, "Total Debit and Credit must be equal.");
            return Page();
        }

        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        using var transaction = conn.BeginTransaction();

        try
        {
            using var cmd = new SqlCommand("sp_SaveVoucher", conn, transaction);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Option", Voucher.Id > 0 ? "UPDATE" : "INSERT");
            cmd.Parameters.AddWithValue("@VoucherType", Voucher.VoucherType);
            cmd.Parameters.AddWithValue("@ReferenceNo", Voucher.ReferenceNo);
            cmd.Parameters.AddWithValue("@VoucherDate", Voucher.VoucherDate);
            cmd.Parameters.AddWithValue("@CreatedBy", Voucher.CreatedBy ?? "Unknown");

            var voucherIdParam = new SqlParameter("@VoucherId", SqlDbType.Int)
            {
                Direction = ParameterDirection.InputOutput,
                Value = Voucher.Id
            };
            cmd.Parameters.Add(voucherIdParam);

            // Prepare TVP param for details
            var tvp = CreateVoucherDetailsDataTable(Voucher.Entries);
            var tvpParam = new SqlParameter("@VoucherDetails", SqlDbType.Structured)
            {
                TypeName = "dbo.VoucherDetailsType",
                Value = tvp
            };
            cmd.Parameters.Add(tvpParam);

            await cmd.ExecuteNonQueryAsync();

            Voucher.Id = (int)voucherIdParam.Value;

            transaction.Commit();

            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            ModelState.AddModelError(string.Empty, "Error saving voucher: " + ex.Message);
            return Page();
        }
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
