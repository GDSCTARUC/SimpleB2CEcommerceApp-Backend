using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OAuthServer.Infrastructure.Models;

namespace OAuthServer.Infrastructure.Context
{
    public class OAuthContext : DbContext, IDataProtectionKeyContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public OAuthContext(DbContextOptions<OAuthContext> options) : base(options)
        {
        }
    }
}
