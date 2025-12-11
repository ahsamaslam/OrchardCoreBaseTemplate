using Microsoft.Extensions.Options;
using Orchard.ModuleBase;

namespace OrchardApp.Host
{
    public class HostMigrationsHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IServiceProvider _sp;

        private readonly ILogger<HostMigrationsHostedService> _logger;
        private readonly IMigrationRunnerService _runner;
        private readonly IOptions<ModuleRegistryOptions> _moduleRegistry;

        public HostMigrationsHostedService(
            IHostApplicationLifetime lifetime,
            IServiceProvider sp,
            ILogger<HostMigrationsHostedService> logger,
            IMigrationRunnerService runner,
            IOptions<ModuleRegistryOptions> moduleRegistry)
        {
            _lifetime = lifetime;
            _sp = sp;
            _logger = logger;
            _runner = runner;
            _moduleRegistry = moduleRegistry;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(() =>
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _sp.CreateScope();
                        _logger.LogInformation("ApplicationStarted (hosted) - found {N} migration assemblies", _moduleRegistry.Value.MigrationAssemblies.Count);

                        // run migrations/provisioning as above
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Post-start operations failed");
                    }
                });
            });

            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    }
}
