using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using MiniAccountManagement.ViewModels;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace MiniAccountManagement.Pages.Vouchers
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ✅ This is the missing property
        public List<VoucherListViewModel> Vouchers { get; set; } = new();

        public async Task OnGetAsync()
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var cmd = new SqlCommand("SELECT * FROM VoucherMaster ORDER BY VoucherDate DESC", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Vouchers.Add(new VoucherListViewModel
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    VoucherType = reader["VoucherType"].ToString(),
                    ReferenceNo = reader["ReferenceNo"].ToString(),
                    VoucherDate = Convert.ToDateTime(reader["VoucherDate"]),
                    CreatedBy = reader["CreatedBy"].ToString(),
                    CreatedOn = Convert.ToDateTime(reader["CreatedOn"])
                });
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await conn.OpenAsync();

                // Delete details first (FK constraint)
                using (var cmd = new SqlCommand("DELETE FROM VoucherDetails WHERE VoucherId = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Delete master
                using (var cmd = new SqlCommand("DELETE FROM VoucherMaster WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage();
        }

    }
}
