using System.Reflection;
namespace Orchard.ModuleBase
{
    public interface IMigrationRunnerService
    {
        Task RunMigrationsAsync(string connectionString, Assembly assembly);
        Task RunMigrationsForTenantAsync(ITenantContext tenantContext);
    }
}
