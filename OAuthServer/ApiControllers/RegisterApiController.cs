using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OAuthServer.Infrastructure.Context;
using OAuthServer.Infrastructure.Models;
using SharedLibrary.Infrastructure.Request;

namespace OAuthServer.ApiControllers
{
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class RegisterApiController : ControllerBase
    {
        private readonly OAuthContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public RegisterApiController(
            OAuthContext context,
            IPasswordHasher<User> passwordHasher
        )
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        [Route("/register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest userRegisterRequest)
        {
            if (string.IsNullOrEmpty(userRegisterRequest.Username))
            {
                return BadRequest("Username is required.");
            }

            if (string.IsNullOrEmpty(userRegisterRequest.FirstName))
            {
                return BadRequest("First name is required.");
            }

            if (string.IsNullOrEmpty(userRegisterRequest.LastName))
            {
                return BadRequest("Last name is required.");
            }

            if (string.IsNullOrEmpty(userRegisterRequest.Email))
            {
                return BadRequest("Email is required.");
            }

            if (string.IsNullOrEmpty(userRegisterRequest.Password) ||
                string.IsNullOrEmpty(userRegisterRequest.PasswordRetype))
            {
                return BadRequest("Password is required.");
            }

            if (userRegisterRequest.Password != userRegisterRequest.PasswordRetype)
            {
                return BadRequest("Password does not match.");
            }

            var newUser = (User)userRegisterRequest;
            newUser.PasswordHashed = _passwordHasher.HashPassword(newUser, userRegisterRequest.Password);

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
