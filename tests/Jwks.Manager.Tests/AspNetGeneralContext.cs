using Jwks.Manager.Store.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jwks.Manager.Tests
{
    public class AspNetGeneralContext : DbContext, IDataProtectionKeyContext, ISecurityKeyContext
    {
        public AspNetGeneralContext(DbContextOptions<AspNetGeneralContext> options)
            : base(options) { }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<SecurityKeyWithPrivate> SecurityKeys { get; set; }
    }
}
