using AuthServer.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Infrastructure.Context;

public class AuthContext : DbContext
{
    public AuthContext(DbContextOptions<AuthContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}