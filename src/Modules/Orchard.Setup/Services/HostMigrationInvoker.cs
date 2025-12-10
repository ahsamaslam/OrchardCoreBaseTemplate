using Orchard.ModuleBase;

namespace Orchard.Setup.Services
{
    public class HostMigrationInvoker
    {
        private readonly IMigrationRunnerService _runner;
        public HostMigrationInvoker(IMigrationRunnerService runner)
        {
            _runner = runner;
        }

        public Task RunHostMigrationsAsync(string connectionString)
        {
            // Example: run migrations for this module's assembly and host-level assemblies
            var asm = typeof(HostMigrationInvoker).Assembly;
            return _runner.RunMigrationsAsync(connectionString, asm);
        }
    }
}
