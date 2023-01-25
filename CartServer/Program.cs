using CartServer.Infrastructure.Context;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=127.0.0.1;Database=CartServer;User=ky;Password=abc123456789;";

builder.Services.AddDbContextPool<CartContext>(options => 
    options.UseMySql(
        defaultConnectionString,
        ServerVersion.AutoDetect(defaultConnectionString)
    ));

builder.Services.AddAuthentication("oauth")
    .AddCookie("oauth", options =>
    {
        options.Cookie.Name = "oauth";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/cartServer/secret", () => "Secret").RequireAuthorization();

await app.RunAsync();