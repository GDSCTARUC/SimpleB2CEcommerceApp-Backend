using CartServer.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CartServer.Infrastructure.Context;

public class CartContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public DbSet<Cart> Carts { get; set; } = null;

    public CartContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }
}