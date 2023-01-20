using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OAuthServer.Infrastructure.Context;
using OAuthServer.Infrastructure.Models;
using SharedLibrary.Infrastructure.Request;
using System.Security.Claims;
using System.Web;

namespace OAuthServer.Controllers
{
    public class LoginController : Controller
    {
        private readonly OAuthContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public LoginController(
            OAuthContext context,
            IPasswordHasher<User> passwordHasher
        )
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        [Route("/login")]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = HttpUtility.UrlEncode(returnUrl);

            return View();
        }

        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> Login(
            [FromQuery] string returnUrl,
            [FromForm] UserLoginRequest userLoginRequest
        )
        {
            if (string.IsNullOrEmpty(userLoginRequest.Username))
            {
                return BadRequest("Username is required");
            }

            if (string.IsNullOrEmpty(userLoginRequest.Password))
            {
                return BadRequest("Password is required");
            }

            var user = await _context.Users
                .Where(m => m.Username == userLoginRequest.Username)
                .SingleOrDefaultAsync();

            if (user == null)
            {
                return BadRequest("User credentials are invalid.");
            }

            var passwordMatch = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHashed,
                userLoginRequest.Password);

            if (passwordMatch == PasswordVerificationResult.Failed)
            {
                return BadRequest("User credeentials are invalid.");
            }

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, "oauth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(
                "oauth",
                claimsPrincipal
            );

            return Redirect(returnUrl ?? "/");
        }
    }
}
