using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OAuthServer.Infrastructure.Context;
using OAuthServer.Infrastructure.Models;
using SharedLibrary.Utils;

namespace OAuthServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=127.0.0.1;Database=OAuthServer;User=ky;Password=abc123456789;";

            builder.Services.AddDbContext<OAuthContext>(options =>
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                ));

            builder.Services.AddDataProtection()
                .SetApplicationName("oauth-server")
                .PersistKeysToDbContext<OAuthContext>();

            builder.Services.AddAuthentication("oauth")
                .AddCookie("oauth", options =>
                {
                    options.Cookie.Name = "oauth";

                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                });

            builder.Services.AddAuthorization();
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.AddSingleton<DevKeys>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }
    }
}