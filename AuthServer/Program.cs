using AuthServer.Constants;
using AuthServer.Infrastructure.Context;
using AuthServer.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=127.0.0.1;Database=AuthServer;User=ky;Password=abc123456789;";

            builder.Services.AddDbContextPool<AuthContext>(options =>
            {
                options.UseMySql(defaultConnectionString, ServerVersion.AutoDetect(defaultConnectionString));
                options.UseOpenIddict();
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(AuthServerCorsDefault.PolicyName, policy =>
                {
                    policy.WithOrigins(AuthServerCorsDefault.CorsOriginHttps, AuthServerCorsDefault.CorsOriginHttp)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
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
                    options.AddEncryptionKey(new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("thisisaveryverylongencryptionkey")));

                    options.AddDevelopmentSigningCertificate();

                    options.AllowClientCredentialsFlow()
                        .AllowAuthorizationCodeFlow()
                        .RequireProofKeyForCodeExchange()
                        .AllowRefreshTokenFlow();

                    options.SetAuthorizationEndpointUris("/connect/authorize")
                        .SetTokenEndpointUris("/connect/token")
                        .SetIntrospectionEndpointUris("/connect/introspect")
                        .SetUserinfoEndpointUris("/connect/userinfo")
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
                        .EnableLogoutEndpointPassthrough();
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
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews();
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

            var app = builder.Build();
            
            if (!app.Environment.IsDevelopment())
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
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "areaRoute",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.UseDeveloperExceptionPage();

            app.Run();
        }
    }
}