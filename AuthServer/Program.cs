using AuthServer.Endpoints;
using AuthServer.Endpoints.OAuth;
using AuthServer.Infrastructure.Context;
using AuthServer.Infrastructure.Models;
using AuthServer.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var defaultConnectionsString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? "Server=127.0.0.1;Database=AuthServer;User=user;Password=abc123456789;";

builder.Services.AddDbContext<AuthContext>(options => options.UseMySql(
    defaultConnectionsString,
    ServerVersion.AutoDetect(defaultConnectionsString)));

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie", options => { options.LoginPath = "/authServer/login"; });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<DevKeys>();

var app = builder.Build();

app.MapPost("/authServer/register", RegisterEndpoint.PostHandler);
app.MapGet("/authServer/login", LoginEndpoint.GetHandler);
app.MapPost("/authServer/login", LoginEndpoint.PostHandler);
app.MapGet("/authServer/oauth/authorize", AuthorizationEndpoint.PostHandler).RequireAuthorization();
app.MapPost("/authServer/oauth/token", TokenEndpoint.PostHandler);

await app.RunAsync();