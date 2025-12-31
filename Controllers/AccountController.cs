using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ACCOB.Models;
using ACCOB.Data;

namespace ACCOB.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return LocalRedirect("/Identity/Account/Login");
        }
    }
}