using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LogoutModel(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPost(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }
        return RedirectToPage("/Account/Login");
    }
}
