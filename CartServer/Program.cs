using CartServer.Constants;
using CartServer.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=127.0.0.1;Database=CartServer;User=ky;Password=abc123456789;";

builder.Services.AddDbContextPool<CartContext>(options =>
    options.UseMySql(
        defaultConnectionString,
        ServerVersion.AutoDetect(defaultConnectionString)
    ));

builder.Services.AddCors(options =>
{
    options.AddPolicy(CartServerCorsDefaults.PolicyName, policy =>
    {
        policy.WithOrigins(CartServerCorsDefaults.CorsOriginHttps, CartServerCorsDefaults.CorsOriginHttp)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer("https://localhost:4000");
        options.AddAudiences("cart_server");

        options.UseIntrospection()
            .SetClientId("cart_server")
            .SetClientSecret("cart_server_secret");

        options.UseSystemNetHttp();

        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors(CartServerCorsDefaults.PolicyName);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/cartServer/secret",() => "Secret").RequireAuthorization();
await app.RunAsync();