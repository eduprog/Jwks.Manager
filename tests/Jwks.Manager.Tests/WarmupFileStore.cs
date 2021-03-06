using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace Jwks.Manager.Tests
{
    /// <summary>
    /// 
    /// </summary>
    public class WarmupFileStore
    {
        public WarmupFileStore()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();

            serviceCollection.AddJwksManager().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory())));

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

    /// <summary>
    /// 
    /// </summary>
    public class WarmupInMemoryStore
    {
        public WarmupInMemoryStore()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();

            serviceCollection.AddJwksManager().PersistKeysInMemory();

            Services = serviceCollection.BuildServiceProvider();
        }
        public ServiceProvider Services { get; set; }
    }
}