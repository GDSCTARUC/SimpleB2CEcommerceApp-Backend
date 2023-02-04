using Microsoft.EntityFrameworkCore;

namespace ProductServer.Infrastructure.Context
{
    public class ProductMySqlContext : ProductContext
    {
        public ProductMySqlContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var defaultConnectionString = Configuration.GetConnectionString("DefaultConnection") + $"Password={Configuration["Product:MariaDBPassword"]};";
            options.UseMySql(defaultConnectionString, 
                ServerVersion.AutoDetect(defaultConnectionString));
        }
    }
}
