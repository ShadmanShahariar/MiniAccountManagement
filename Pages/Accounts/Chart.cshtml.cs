using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountManagement.ViewModels;
using System.Data;

namespace MiniAccountManagement.Pages.Accounts
{
    public class ChartModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ChartModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<ChartOfAccountViewModel> Accounts { get; set; }

        public async Task<IActionResult> OnGetAsync()
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
                                AccountName = reader["AccountName"].ToString(),
                                ParentId = reader["ParentId"] as int?,
                                AccountType = reader["AccountType"].ToString()
                            });
                        }
                    }
                }
            }

            return Page();
        }
    }

}
