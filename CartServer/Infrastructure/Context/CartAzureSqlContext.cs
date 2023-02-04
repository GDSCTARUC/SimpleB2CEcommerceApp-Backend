using Microsoft.EntityFrameworkCore;

namespace CartServer.Infrastructure.Context
{
    public class CartAzureSqlContext : CartContext
    {
        public CartAzureSqlContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(Configuration.GetConnectionString("AzureSqlConnection"));
        }
    }
}
