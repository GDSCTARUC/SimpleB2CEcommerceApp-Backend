using AuthServer.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Infrastructure.Context
{
    public class AuthContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public AuthContext(DbContextOptions<AuthContext> options) : base(options)
        {
        }
    }
}
