using AuthServer.Infrastructure.Context;
using AuthServer.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Infrastructure.Request;
using System.Security.Claims;
using System.Web;

namespace AuthServer.Endpoints;

public static class LoginEndpoint
{
    public static async Task GetHandler(HttpResponse httpResponse, string returnUrl)
    {
        httpResponse.Headers.ContentType = new string[] { "text/html" };
        await httpResponse.WriteAsync(
$"""
<html>
    <head>
        <title>Login</title>
    </head>
    <body>
        <form action="/authServer/login?returnUrl={HttpUtility.UrlEncode(returnUrl)}" method="post">
            <input name="username" value="Test_User_01" />
            <input name="password" value="abc123456789" />
            <input value="Submit" type="submit" />
        </form>
    </body>
</html>
"""     );
    }

    public static async Task<IResult> PostHandler(
        [FromForm] UserLoginRequest userLoginRequest,
        HttpContext httpContext,
        AuthContext db,
        IPasswordHasher<User> hasher)
    {
        if (string.IsNullOrEmpty(userLoginRequest.Username)) return Results.BadRequest("Username cannot be empty.");

        if (string.IsNullOrEmpty(userLoginRequest.Password)) return Results.BadRequest("Password cannot be empty.");

        var user = await db.Users
            .Where(m => m.Username == userLoginRequest.Username)
            .FirstOrDefaultAsync();

        if (user == null) return Results.BadRequest("User credentials invalid");

        var passwordMatched = hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            userLoginRequest.Password
        );

        if (passwordMatched == PasswordVerificationResult.Failed) return Results.BadRequest("User credentials invalid");

        var claims = new Claim[]
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, "cookie");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await httpContext.SignInAsync("cookie", claimsPrincipal);

        return Results.Redirect(userLoginRequest.RedirectUrl ?? "/");
    }
}