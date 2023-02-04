using Microsoft.EntityFrameworkCore;

namespace ProductServer.Infrastructure.Context
{
    public class ProductAzureSqlContext : ProductContext
    {
        public ProductAzureSqlContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(Configuration.GetConnectionString("AzureSqlConnection"));
        }
    }
}
