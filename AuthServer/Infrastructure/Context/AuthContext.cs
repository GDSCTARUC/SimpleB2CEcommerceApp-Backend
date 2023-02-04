using AuthServer.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Infrastructure.Context;

public class AuthContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public DbSet<User> Users { get; set; }

    public AuthContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }
}