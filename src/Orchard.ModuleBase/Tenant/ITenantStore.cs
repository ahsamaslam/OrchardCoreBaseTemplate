// Host/Tenants/ITenantStore.cs
using Orchard.ModuleBase.Tenant;

public interface ITenantStore
{
    Task<ITenantContext?> FindByHostAsync(string host);
    Task AddTenantAsync(ITenantContext tenant);
    Task<ITenantContext?> FindByIdAsync(string tenantId);
    Task<IEnumerable<ITenantContext>> ListAsync();
}
