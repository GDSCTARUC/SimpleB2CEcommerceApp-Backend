using AuthServer.Infrastructure.Context;
using AuthServer.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthServer
{
    public class UserWorker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public UserWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AuthContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

            if (context.Users.Any())
                return;

            var adminUser = new User
            {
                Username = "admin",
                FirstName = "Admin",
                LastName = "Admin",
                Email = "admin@web.com",
                IsAdmin = true
            };

            adminUser.PasswordHashed = passwordHasher.HashPassword(adminUser, "abc123456789");

            await context.Users.AddAsync(adminUser, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
