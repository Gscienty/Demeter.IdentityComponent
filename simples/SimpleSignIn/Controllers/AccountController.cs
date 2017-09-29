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
        public ActionResult SignIn() => View();

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> SignIn(RegisterModel registerModel)
        {
            DemeterUserIdentity user = new DemeterUserIdentity(registerModel.Username);
            
            user.SetProfile("simple_profile", new ProfileModel
            {
                Address = registerModel.Address,
                RealName = registerModel.Realname
            });

            await this._userManager.CreateAsync(user, registerModel.Password);

            return RedirectToAction("Index", "Home");
        }
    }
}