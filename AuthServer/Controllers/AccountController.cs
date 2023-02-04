using System.Security.Claims;
using AuthServer.Infrastructure.Context;
using AuthServer.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Infrastructure.Requests;

namespace AuthServer.Controllers;

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
    public IActionResult Login([FromQuery] string returnUrl, [FromQuery] string from)
    {
        ViewBag.RegisterSuccessMessage = null;
        ViewBag.ErrorMessage = null;

        if (from == "registerSuccess")
            ViewBag.RegisterSuccessMessage = "You successfully register an account, you can now login!";

        return View(new UserLoginRequest
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpGet]
    public IActionResult Register()
    {
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
        if (!ModelState.IsValid) return View(userLoginRequest);

        var user = await _context.Users
            .SingleOrDefaultAsync(x => x.Username == userLoginRequest.Username);

        if (user == null)
        {
            ViewBag.ErrorMessage = "User credentials invalid";
            return View();
        }

        var passwordVerificationResult =
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHashed, userLoginRequest.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
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
        if (!ModelState.IsValid) return View(userRegisterRequest);

        if (userRegisterRequest.Password != userRegisterRequest.PasswordRetype)
            ViewBag.ErrorMessage = "Password does not match with password retype";

        var newUser = (User)userRegisterRequest;
        newUser.PasswordHashed = _passwordHasher.HashPassword(newUser, userRegisterRequest.Password);

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Login), "Account", new
        {
            from = "registerSuccess"
        });
    }
}