using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountManagement.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class ModulePermissionsModel : PageModel
{
    private readonly IConfiguration _config;
    public ModulePermissionsModel(IConfiguration config) => _config = config;

    [BindProperty]
    public string RoleName { get; set; }

    [BindProperty]
    public List<ModulePermissionViewModel> Permissions { get; set; }

    public async Task OnGetAsync(string roleName)
    {
        RoleName = roleName;
        Permissions = new();

        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        using var cmd = new SqlCommand("sp_GetAllModulesWithPermissions", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@RoleName", roleName);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Permissions.Add(new ModulePermissionViewModel
            {
                ModuleId = (int)reader["Id"],
                ModuleName = reader["Name"].ToString(),
                IsAllowed = (bool)reader["IsAllowed"]
            });
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        foreach (var permission in Permissions)
        {
            using var cmd = new SqlCommand("sp_UpdateRoleModulePermission", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@RoleName", RoleName);
            cmd.Parameters.AddWithValue("@ModuleId", permission.ModuleId);
            cmd.Parameters.AddWithValue("@IsAllowed", permission.IsAllowed);
            await cmd.ExecuteNonQueryAsync();
        }

        TempData["Success"] = "Permissions updated.";
        return RedirectToPage(new { roleName = RoleName });
    }
}
