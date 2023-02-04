using Microsoft.EntityFrameworkCore;

namespace AuthServer.Infrastructure.Context
{
    public class AuthMySqlContext : AuthContext
    {
        public AuthMySqlContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var defaultConnectionString = Configuration.GetConnectionString("DefaultConnection") + $"Password={Configuration["Auth:MariaDBPassword"]};";
            options.UseMySql(defaultConnectionString, 
                ServerVersion.AutoDetect(defaultConnectionString));

            options.UseOpenIddict();
        }
    }
}
