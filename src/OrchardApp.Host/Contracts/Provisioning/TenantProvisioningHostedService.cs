using Microsoft.Extensions.Options;
using Orchard.ModuleBase;
using Orchard.ModuleBase.Tenant;

namespace OrchardApp.Host.Contracts.Provisioning
{
    public class TenantProvisioningHostedService : BackgroundService
    {
        private readonly IProvisionQueue _queue;
        private readonly ITenantDatabaseCreator _creator;
        private readonly IMigrationRunnerService _orchestrator;
        private readonly ILogger<TenantProvisioningHostedService> _logger;
        private readonly IOptions<ModuleRegistryOptions> _modules;

        public TenantProvisioningHostedService(IProvisionQueue queue, ITenantDatabaseCreator creator, IMigrationRunnerService orchestrator, ILogger<TenantProvisioningHostedService> logger, IOptions<ModuleRegistryOptions> modules)
        { _queue = queue; _creator = creator; _orchestrator = orchestrator; _logger = logger; _modules = modules; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var item = await _queue.DequeueAsync(stoppingToken);
                if (item == null) continue;
                var taskId = item.Id;
                try
                {
                    _logger.LogInformation("Provisioning tenant {Id}", item.Tenant.TenantId);
                    await _creator.EnsureDatabaseExistsAsync(item.Tenant.ConnectionString, stoppingToken);
                    var assemblies = _modules.Value.MigrationAssemblies;
                    // use modules registered assemblies
                    await _orchestrator.RunMigrationsForTenantAsync(item.Tenant);
                    _logger.LogInformation("Provisioning succeeded for {Id}", item.Tenant.TenantId);
                    // persist provisioning status -> ISettingsStore or DB (omitted here)
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Provisioning failed for {Id}", item.Tenant.TenantId);
                    // persist failed status
                }
            }
        }
    }
}
