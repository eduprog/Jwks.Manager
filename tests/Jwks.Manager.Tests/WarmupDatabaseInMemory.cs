using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jwks.Manager.Tests
{
    /// <summary>
    /// 
    /// </summary>
    public class WarmupDatabaseInMemory
    {
        public WarmupDatabaseInMemory()
        {
            var serviceCollection = new ServiceCollection();

            void DatabaseOptions(DbContextOptionsBuilder opt) => opt.UseInMemoryDatabase("JpTests").EnableSensitiveDataLogging();

            serviceCollection.AddLogging();
            serviceCollection.AddDbContext<AspNetGeneralContext>(DatabaseOptions);

            serviceCollection.AddJwksManager().PersistKeysToDatabaseStore<AspNetGeneralContext>();

            Services = serviceCollection.BuildServiceProvider();
        }
        public ServiceProvider Services { get; set; }

        public void DetachAll()
        {

            var database = Services.GetService<AspNetGeneralContext>();
            foreach (var dbEntityEntry in database.ChangeTracker.Entries())
            {
                if (dbEntityEntry.Entity != null)
                {
                    dbEntityEntry.State = EntityState.Detached;
                }
            }

        }
    }
}
