using Microsoft.EntityFrameworkCore;
using ProductServer.Infrastructure.Models;

namespace ProductServer.Infrastructure.Context;

public class ProductContext : DbContext
{
    public ProductContext(DbContextOptions<ProductContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
}