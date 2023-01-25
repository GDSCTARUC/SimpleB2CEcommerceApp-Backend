using AuthServer.Infrastructure.Context;
using AuthServer.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Server.AspNetCore;
using SharedLibrary.Infrastructure.Requests;
using System.Security.Claims;

namespace AuthServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(AuthContext authContext, IPasswordHasher<User> passwordHasher)
        {
            _context = authContext;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Login([FromQuery] string returnUrl)
        {
            ViewBag.Error = false;
            ViewBag.ErrorMessage = null;
            
            return View(new UserLoginRequest
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Error = false;
            ViewBag.ErrorMessage = null;

            return View();
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] UserLoginRequest userLoginRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(userLoginRequest);
            }

            var user = await _context.Users
                .SingleOrDefaultAsync(x => x.Username == userLoginRequest.Username);

            if (user == null)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = "User credentials invalid";
                return View();
            }

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHashed, userLoginRequest.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = "User credentials invalid";
                return View();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));

            return !string.IsNullOrWhiteSpace(userLoginRequest.ReturnUrl) 
                ? Redirect(userLoginRequest.ReturnUrl) 
                : RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] UserRegisterRequest userRegisterRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(userRegisterRequest);
            }

            if (userRegisterRequest.Password != userRegisterRequest.PasswordRetype)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = "Password does not match with password retype";
            }

            var newUser = (User)userRegisterRequest;
            newUser.PasswordHashed = _passwordHasher.HashPassword(newUser, userRegisterRequest.Password);

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AccountController.Login), "Account");
        }
    }
}
