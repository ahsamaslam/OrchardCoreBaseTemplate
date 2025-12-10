using Orchard.ModuleBase;

namespace OrchardApp.Host
{
    internal class HostMigrationsHostedService : IHostedService
    {
        private readonly ILogger<HostMigrationsHostedService> _logger;
        private readonly IMigrationRunnerService _runner;
        private readonly Microsoft.Extensions.Options.IOptions<ModuleRegistryOptions> _moduleRegistry;
        private readonly string _hostConnectionString;

        public HostMigrationsHostedService(
            ILogger<HostMigrationsHostedService> logger,
            IMigrationRunnerService runner,
            Microsoft.Extensions.Options.IOptions<ModuleRegistryOptions> moduleRegistry,
            string hostConnectionString)
        {
            _logger = logger;
            _runner = runner;
            _moduleRegistry = moduleRegistry;
            _hostConnectionString = hostConnectionString;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Run host-level migrations for each registered module assembly
            var assemblies = _moduleRegistry.Value.MigrationAssemblies;
            if (assemblies == null || assemblies.Count == 0)
            {
                _logger.LogInformation("No module migration assemblies registered — skipping host migrations.");
                return;
            }

            _logger.LogInformation("Starting host migrations for {Count} assemblies", assemblies.Count);
            foreach (var asm in assemblies)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    _logger.LogInformation("Running migrations for assembly {Assembly}", asm.FullName);
                    await _runner.RunMigrationsAsync(_hostConnectionString, asm);
                    _logger.LogInformation("Completed migrations for assembly {Assembly}", asm.FullName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to run migrations for assembly {Assembly}", asm.FullName);
                    // Decide whether to rethrow or continue. Rethrow will prevent app from starting.
                    throw;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
