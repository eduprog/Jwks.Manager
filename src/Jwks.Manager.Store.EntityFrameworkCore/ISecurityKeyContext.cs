using Microsoft.EntityFrameworkCore;

namespace Jwks.Manager.Store.EntityFrameworkCore
{
    public interface ISecurityKeyContext
    {
        /// <summary>
        /// A collection of <see cref="T:Jwks.Manager.SecurityKeyWithPrivate" />
        /// </summary>
        DbSet<SecurityKeyWithPrivate> SecurityKeys { get; set; }
    }
}
