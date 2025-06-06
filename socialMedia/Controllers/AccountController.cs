﻿
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using socialMedia.Core.Domain;
using socialMedia.Infrastructure;
using socialMedia.Shared.DTO;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Principal;

namespace social.Controllers
{

    public class AccountController : Controller
    {
        public RoleManager<IdentityRole> RoleManager { get; set; }
        public UserManager<ApplicationUser> UserManager { get; }
        public SignInManager<ApplicationUser> SignInManager { get; }

        public readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context )
        {
            this.UserManager = userManager;
            this.SignInManager = signInManager;
            this.RoleManager = roleManager;
            this._context = context;
        }
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Login(LogInDTO login)

        {

            if (ModelState.IsValid)

            {

                var user = await UserManager.FindByEmailAsync(login.Email);

                if (user != null)

                {

                    if (user.IsActive != false)

                    {

                        var isCredentail = await UserManager.CheckPasswordAsync(user, login.Password);

                        if (isCredentail)

                        {

                            var roles = await UserManager.GetRolesAsync(user);

                            var claims = new List<Claim>

                    {

                        new Claim("UserName",$"{user.FirstName} {user.LastName}" ),

                        new Claim(ClaimTypes.NameIdentifier, user.Id),

                        new Claim(ClaimTypes.Email, user.Email)

                    };

                            // Add role claims

                            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                            var principal = new ClaimsPrincipal(claimsIdentity);

                            await SignInManager.SignOutAsync();

                            await SignInManager.SignInWithClaimsAsync(user, isPersistent: login.RememberMe, claims);

                            if (roles.Contains("superadmin"))
                            {
                                return RedirectToAction("Index", "Admin");
                            }
                            else if (roles.Contains("Employee"))
                            {
                                return RedirectToAction("MyProjects", "Employee");
                            }
                            else if (roles.Contains("Client"))
                            {
                                return RedirectToAction("Index", "Client");
                            }
                            else
                            {
                                // Default or unauthorized role fallback
                                return RedirectToAction("AccessDenied", "Account");
                            }
                        }

                        else

                        {

                            ModelState.AddModelError("", "Invalid login attempt.");

                        }

                    }

                    else

                    {

                        ModelState.AddModelError("", "User is not Approved by Admin");

                    }

                }

                else

                {

                    ModelState.AddModelError("", "User not found.");

                }

                return View(login);

            }

            return View(login);

        }


        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            SignInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(UserDTO user)
        {
            if (ModelState.IsValid)
            {
                var IdentityUser = new ApplicationUser();
                IdentityUser.Email = user.Email;
                IdentityUser.FirstName = user.FirstName;
                IdentityUser.LastName = user.LastName;
                IdentityUser.UserName = user.Email;
                IdentityUser.IsActive = false;
                var isUserCreated = await UserManager.CreateAsync(IdentityUser, user.Password);
                if (isUserCreated.Succeeded)
                {

                    ModelState.AddModelError("", "Your Are Registered Successfully, Please wait for Admin to approve you account");
                    return View();
                }
                else
                {
                    foreach (var Error in isUserCreated.Errors)
                    {
                        ModelState.AddModelError("", Error.Description);
                    }

                }

            }
            return View(user);
        }
       


    }
}
