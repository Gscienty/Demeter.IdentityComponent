using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SimpleSignIn.Models;
using Demeter.IdentityComponent;

namespace SimpleSignIn.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<DemeterUserIdentity> _userManager;

        public HomeController(
            UserManager<DemeterUserIdentity> userManager)
        {
            this._userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.LoginState = "already logged";
                var user = await this._userManager.GetUserAsync(User);

                ViewBag.UserName = user.UserName;
                ViewBag.Address = user.GetProfile<ProfileModel>("simple_profile").Address;
                ViewBag.RealName = user.GetProfile<ProfileModel>("simple_profile").RealName;
            }
            else
            {
                ViewBag.LoginState = "Not logged in";
            }
            
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
