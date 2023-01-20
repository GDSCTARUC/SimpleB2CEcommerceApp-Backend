using Microsoft.AspNetCore.Authentication;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("oauth")
    .AddCookie("oauth", options =>
    {
        options.Cookie.Name = "oauth";
    })
    .AddOAuth("oauth-server", options =>
    {
        options.SignInScheme = "oauth";

        options.ClientId = "x";
        options.ClientSecret = "x";

        options.AuthorizationEndpoint = "https://localhost:4000/oauth/authorize";
        options.TokenEndpoint = "https://localhost:4000/oauth/token";
        options.CallbackPath = "/oauth/process";

        options.UsePkce = true;
        options.ClaimActions.MapJsonKey("sub", "sub");
        options.Events.OnCreatingTicket = async ctx =>
        {
            var payloadBase64 = ctx.AccessToken.Split(".")[1];
            var payloadJson = Base64UrlTextEncoder.Decode(payloadBase64);
            var payload = JsonDocument.Parse(payloadJson);
            ctx.RunClaimActions(payload.RootElement);
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.MapGet("/", (HttpContext ctx) =>
{
    return ctx.User.Claims.Select(x => new { x.Type, x.Value }).ToList();
});

app.MapGet("/login", () =>
{
    return Results.Challenge(
        new AuthenticationProperties
        {
            RedirectUri = "https://localhost:3000/"
        },
        authenticationSchemes: new List<string> { "oauth-server" }
    );
});

app.Run();