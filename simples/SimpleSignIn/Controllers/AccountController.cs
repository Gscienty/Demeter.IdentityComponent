using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Demeter.IdentityComponent;
using SimpleSignIn.Models;

namespace SimpleSignIn.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<DemeterUserIdentity> _signInManager;
        private readonly UserManager<DemeterUserIdentity> _userManager;

        public AccountController(
            SignInManager<DemeterUserIdentity> signInManager,
            UserManager<DemeterUserIdentity> userManager)
        {
            this._signInManager = signInManager;
            this._userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register() => View();

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Register(RegisterModel registerModel)
        {
            DemeterUserIdentity user = new DemeterUserIdentity(registerModel.Username);
            
            user.SetProfile("simple_profile", new ProfileModel
            {
                Address = registerModel.Address,
                RealName = registerModel.Realname
            });
            IdentityResult result = await this._userManager.CreateAsync(user, registerModel.Password);

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login() => View();

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Login(LoginModel loginModel)
        {
            var result = await this._signInManager.PasswordSignInAsync(
                loginModel.Username,
                loginModel.Password,
                false,
                false);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            await this._signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}