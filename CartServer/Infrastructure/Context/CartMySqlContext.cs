using Microsoft.EntityFrameworkCore;

namespace CartServer.Infrastructure.Context
{
    public class CartMySqlContext : CartContext
    {
        public CartMySqlContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var defaultConnectionString = Configuration.GetConnectionString("DefaultConnection") + $"Password={Configuration["Cart:MariaDBPassword"]};";
            options.UseMySql(defaultConnectionString, 
                ServerVersion.AutoDetect(defaultConnectionString));
        }
    }
}
