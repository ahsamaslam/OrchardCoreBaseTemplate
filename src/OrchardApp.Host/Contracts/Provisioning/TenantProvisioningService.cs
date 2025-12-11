using Orchard.ModuleBase;
using Orchard.ModuleBase.Tenant;
namespace OrchardApp.Host.Contracts.Provisioning
{
    public class TenantProvisioningService : ITenantProvisioningService
    {
        private readonly IProvisionQueue _queue;
        // TODO: add status persistence (ISettingsStore or DB)
        public TenantProvisioningService(IProvisionQueue queue) => _queue = queue;

        public async Task<string> EnqueueProvisionAsync(TenantContext tenant, ProvisionOptions options, CancellationToken ct = default)
        {
            var item = new ProvisionQueueItem { Tenant = tenant, Options = options };
            await _queue.EnqueueAsync(item);
            // set initial status: enqueued
            return item.Id;
        }

        public Task<ProvisioningStatus?> GetStatusAsync(string taskId) => Task.FromResult<ProvisioningStatus?>(null); // implement via ISettingsStore
        public Task<ProvisioningStatus?> GetStatusByTenantAsync(string tenantId) => Task.FromResult<ProvisioningStatus?>(null);
    }
}
