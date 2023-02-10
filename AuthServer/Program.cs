using System.Text;
using AuthServer.Constants;
using AuthServer.Infrastructure.Context;
using AuthServer.Infrastructure.Models;
using AuthServer.Infrastructure.Policies;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        switch (builder.Configuration["DatabaseProvider"])
        {
            case "MySql":
                builder.Services.AddDbContext<AuthContext, AuthMySqlContext>();
                break;

            case "AzureSql":
                builder.Services.AddDbContext<AuthContext, AuthAzureSqlContext>();
                break;
        }

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(AuthServerCorsDefault.PolicyName, policy =>
            {
                policy.WithOrigins(AuthServerCorsDefault.CorsOriginHttps, AuthServerCorsDefault.CorsOriginHttp, "https://icy-flower-09eb00c00.2.azurestaticapps.net")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        builder.Services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<AuthContext>();
            })
            .AddServer(options =>
            {
                if (builder.Environment.IsProduction())
                {
                    options.AddEncryptionCertificate("26C6F6AC1B25896897083A79692087A01D0F50D1")
                        .AddSigningCertificate("26C6F6AC1B25896897083A79692087A01D0F50D1");
                }
                else
                {
                    options.AddEncryptionKey(new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("thisisaveryverylongencryptionkey")));

                    options.AddDevelopmentSigningCertificate();
                }

                options.AllowClientCredentialsFlow()
                    .AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange()
                    .AllowRefreshTokenFlow();

                options.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetIntrospectionEndpointUris("/connect/introspect")
                    .SetUserinfoEndpointUris("/connect/userinfo")
                    .SetVerificationEndpointUris("/connect/verify")
                    .SetLogoutEndpointUris("/connect/logout");

                options.RegisterScopes(
                    Scopes.Profile,
                    Scopes.Email,
                    Scopes.Roles,
                    Scopes.Phone,
                    Scopes.Address,
                    Scopes.Roles,
                    "cart_api",
                    "product_api");

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();

                options.UseAspNetCore();
            });

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "auth";
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logout";
            });

        builder.Services.AddAuthorization();
        builder.Services.AddRazorPages()
            .AddRazorRuntimeCompilation();

        builder.Services.AddControllersWithViews()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy());

        builder.Services.AddHostedService<OpenIddictWorker>();
        builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

        if (builder.Environment.IsProduction())
        {
            builder.Services.AddHostedService<UserWorker>();
        }
        
        var app = builder.Build();

        if (app.Environment.IsProduction())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        } 

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseCors(AuthServerCorsDefault.PolicyName);
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllerRoute(
            "default",
            "{controller=Home}/{action=Index}/{id?}");
        app.MapControllerRoute(
            "areaRoute",
            "{area:exists}/{controller=Home}/{action=Index}/{id?}");
        app.UseDeveloperExceptionPage();

        await app.RunAsync();
    }
}