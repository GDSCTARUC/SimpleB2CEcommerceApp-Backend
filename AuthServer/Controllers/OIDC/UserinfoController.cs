using AuthServer.Infrastructure.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SharedLibrary.Infrastructure.DataTransferObjects;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer.Controllers.OIDC;

public class UserinfoController : Controller
{
    private readonly AuthContext _context;

    public UserinfoController(AuthContext context)
    {
        _context = context;
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    public async Task<ActionResult<UserDto>> UserInfo()
    {
        var claimsPrincipal =
            (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        if (!int.TryParse(claimsPrincipal.Claims.FirstOrDefault(m => m.Type == Claims.Subject).Value, out var userId))
            return BadRequest();

        var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == userId);
        if (user == null) return BadRequest();

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = user.Id.ToString()
        };

        if (User.HasScope(Scopes.Profile))
            claims[Claims.Profile] = new
            {
                user.Username,
                user.FirstName,
                user.LastName,
                user.IsAdmin
            };

        if (User.HasScope(Scopes.Address))
        {
        }

        if (User.HasScope(Scopes.Email))
        {
            claims[Claims.Email] = user.Email;
            claims[Claims.EmailVerified] = false;
        }

        if (User.HasScope(Scopes.Phone))
        {
        }

        if (User.HasScope(Scopes.Roles))
        {
        }

        return Ok(claims);
    }
}