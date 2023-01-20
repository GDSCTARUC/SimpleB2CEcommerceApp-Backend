using AuthServer.Infrastructure.Context;
using AuthServer.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Infrastructure.Request;

namespace AuthServer.Endpoints;

public static class RegisterEndpoint
{
    public static async Task<IResult> PostHandler(
        AuthContext db,
        IPasswordHasher<User> hasher,
        [FromBody] UserRegisterRequest userRegisterRequest
    )
    {
        if (string.IsNullOrEmpty(userRegisterRequest.Username)) return Results.BadRequest("Username cannot be empty.");

        if (string.IsNullOrEmpty(userRegisterRequest.FirstName))
            return Results.BadRequest("First name cannot be empty.");

        if (string.IsNullOrEmpty(userRegisterRequest.LastName)) return Results.BadRequest("Last name cannot be empty.");

        if (string.IsNullOrEmpty(userRegisterRequest.Email)) return Results.BadRequest("Email cannot be empty.");

        if (string.IsNullOrEmpty(userRegisterRequest.Password) ||
            string.IsNullOrEmpty(userRegisterRequest.PasswordRetype)
           )
            return Results.BadRequest("Password cannot be empty.");

        if (userRegisterRequest.Password != userRegisterRequest.PasswordRetype)
            return Results.BadRequest("Password does not match.");

        var newUser = new User
        {
            Username = userRegisterRequest.Username,
            FirstName = userRegisterRequest.FirstName,
            LastName = userRegisterRequest.LastName,
            Email = userRegisterRequest.Email
        };

        var passwordHashed = hasher.HashPassword(newUser, userRegisterRequest.Password);
        newUser.PasswordHash = passwordHashed;

        _ = await db.AddAsync(newUser);

        return await db.SaveChangesAsync() > 0
            ? Results.Ok()
            : Results.BadRequest("Register user failed, please try again");
    }
}