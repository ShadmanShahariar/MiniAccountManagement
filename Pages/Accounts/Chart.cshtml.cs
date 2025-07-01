using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using MiniAccountManagement.ViewModels;

public class ChartModel : PageModel
{
    private readonly IConfiguration _configuration;

    public ChartModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty]
    public ChartOfAccountViewModel FormModel { get; set; } = new();

    public List<ChartOfAccountViewModel> Accounts { get; set; } = new();
    public List<SelectListItem> ParentAccountList { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAccounts();
        return Page();
    }

    public async Task<IActionResult> OnGetEditAsync(int id)
    {
        var flatList = new List<ChartOfAccountViewModel>();

        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@Option", "GET");

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var account = new ChartOfAccountViewModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                AccountName = reader["AccountName"].ToString(),
                ParentId = reader["ParentId"] as int?,
                AccountType = reader["AccountType"].ToString()
            };

            flatList.Add(account);

            if (account.Id == id)
                FormModel = account;
        }

        Accounts = BuildTree(flatList);
        ParentAccountList = GetFlatAccountList(Accounts);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@Option", FormModel.Id == 0 ? "INSERT" : "UPDATE");
        cmd.Parameters.AddWithValue("@Id", FormModel.Id);
        cmd.Parameters.AddWithValue("@AccountName", FormModel.AccountName);
        cmd.Parameters.AddWithValue("@ParentId", (object?)FormModel.ParentId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AccountType", FormModel.AccountType);

        await cmd.ExecuteNonQueryAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@Option", "DELETE");
        cmd.Parameters.AddWithValue("@Id", id);

        await cmd.ExecuteNonQueryAsync();
        return RedirectToPage();
    }

    private async Task LoadAccounts()
    {
        var flatList = new List<ChartOfAccountViewModel>();

        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@Option", "GET");

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            flatList.Add(new ChartOfAccountViewModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                AccountName = reader["AccountName"].ToString(),
                ParentId = reader["ParentId"] as int?,
                AccountType = reader["AccountType"].ToString()
            });
        }

        Accounts = BuildTree(flatList);
        ParentAccountList = GetFlatAccountList(Accounts);
    }

    private List<ChartOfAccountViewModel> BuildTree(List<ChartOfAccountViewModel> flatList)
    {
        var lookup = flatList.ToDictionary(x => x.Id);
        var root = new List<ChartOfAccountViewModel>();

        foreach (var item in flatList)
            item.Children = new List<ChartOfAccountViewModel>();

        foreach (var item in flatList)
        {
            if (item.ParentId.HasValue && lookup.ContainsKey(item.ParentId.Value))
                lookup[item.ParentId.Value].Children.Add(item);
            else
                root.Add(item);
        }

        return root;
    }

    private List<SelectListItem> GetFlatAccountList(List<ChartOfAccountViewModel> tree, string prefix = "")
    {
        var list = new List<SelectListItem>();
        foreach (var node in tree)
        {
            list.Add(new SelectListItem
            {
                Value = node.Id.ToString(),
                Text = prefix + node.AccountName
            });

            if (node.Children.Any())
                list.AddRange(GetFlatAccountList(node.Children, prefix + "-- "));
        }
        return list;
    }
}
