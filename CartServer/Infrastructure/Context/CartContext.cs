using CartServer.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CartServer.Infrastructure.Context;

public class CartContext : DbContext
{
    public CartContext(DbContextOptions<CartContext> options) : base(options)
    {
    }

    public DbSet<Cart> Carts { get; set; } = null;
}