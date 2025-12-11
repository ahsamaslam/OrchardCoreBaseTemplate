using Orchard.ModuleBase.Tenant;

namespace OrchardApp.Host.Contracts.Provisioning
{
    public interface IProvisioningStatusStore
    {
        Task<ProvisioningStatus?> GetStatusAsync(string tenantId, CancellationToken cancellationToken = default);
        Task SaveStatusAsync(string tenantId, ProvisioningStatus status, CancellationToken cancellationToken = default);
        Task ClearStatusAsync(string tenantId, CancellationToken cancellationToken = default);
    }
}
