using Microsoft.EntityFrameworkCore;
using ProductServer.Infrastructure.Models;

namespace ProductServer.Infrastructure.Context;

public class ProductContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public DbSet<Product> Products { get; set; }

    public ProductContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }
}