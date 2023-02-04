using Microsoft.EntityFrameworkCore;

namespace AuthServer.Infrastructure.Context
{
    public class AuthAzureSqlContext : AuthContext
    {
        public AuthAzureSqlContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(Configuration.GetConnectionString("AzureSqlConnection"));

            options.UseOpenIddict();
        }
    }
}
