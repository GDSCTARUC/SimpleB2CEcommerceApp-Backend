using Microsoft.EntityFrameworkCore;

namespace CartServer.Infrastructure.Context
{
    public class CartContext : DbContext
    {
        public CartContext(DbContextOptions<CartContext> options) : base(options)
        {
        }
    }
}
