using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

public class UsersModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public List<UserWithRoles> Users { get; set; } = new();
    public SelectList RoleList { get; set; }
    [BindProperty]
    public NewUserModel NewUser { get; set; }
    public string Message { get; set; }

    public class NewUserModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }
    }

    public class UserWithRoles
    {
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadUsersAndRolesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadUsersAndRolesAsync();
            return Page();
        }

        var user = new IdentityUser { UserName = NewUser.Email, Email = NewUser.Email };
        var result = await _userManager.CreateAsync(user, NewUser.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, NewUser.Role);
            Message = "User created successfully.";
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        await LoadUsersAndRolesAsync();
        return Page();
    }

    private async Task LoadUsersAndRolesAsync()
    {
        RoleList = new SelectList(await _roleManager.Roles.Select(r => r.Name).ToListAsync());
        Users = new List<UserWithRoles>();

        foreach (var user in _userManager.Users.ToList())
        {
            var roles = await _userManager.GetRolesAsync(user);
            Users.Add(new UserWithRoles
            {
                Email = user.Email,
                Roles = roles.ToList()
            });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToPage();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Error deleting user.");
            }
        }

        return RedirectToPage();
    }

}
