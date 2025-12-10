using Orchard.ModuleBase;
using Orchard.TenantManagement.Services;

namespace OrchardApp.Host.Tenants
{ 

    /// <summary>
    /// Adapts the database-backed ITenantRepository from the TenantManagement module
    /// into the ITenantStore used by TenantResolutionMiddleware and the rest of the Host.
    /// </summary>
    public class HostTenantStoreAdapter : ITenantStore
    {
        private readonly ITenantRepository _repo;

        public HostTenantStoreAdapter(ITenantRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Find tenant by hostname. Supports multiple comma-separated hostnames.
        /// </summary>
        public async Task<ITenantContext?> FindByHostAsync(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return null;

            var tenantInfo = await _repo.FindByHostAsync(host);
            if (tenantInfo == null) return null;

            return new TenantContext(
                tenantId: tenantInfo.TenantId,
                tenantName: tenantInfo.TenantName,
                connectionString: tenantInfo.ConnectionString,
                settings: tenantInfo.GetSettingsDictionary()
            );
        }

        public async Task<ITenantContext?> FindByIdAsync(string tenantId)
        {
            var tenantInfo = await _repo.FindByIdAsync(tenantId);
            if (tenantInfo == null) return null;

            return new TenantContext(
                tenantId: tenantInfo.TenantId,
                tenantName: tenantInfo.TenantName,
                connectionString: tenantInfo.ConnectionString,
                settings: tenantInfo.GetSettingsDictionary()
            );
        }

        public async Task<IEnumerable<ITenantContext>> ListAsync()
        {
            var list = await _repo.ListAsync();
            return list.Select(t => new TenantContext(
                tenantId: t.TenantId,
                tenantName: t.TenantName,
                connectionString: t.ConnectionString,
                settings: t.GetSettingsDictionary()
            ));
        }

        // HostTenantStoreAdapter does not create or remove tenants — TenantManagement handles that.
        public Task AddTenantAsync(ITenantContext tenant) =>
            throw new System.NotSupportedException("Use TenantManagement API to create tenants.");

        public Task<bool> RemoveTenantAsync(string tenantId) =>
            throw new System.NotSupportedException("Use TenantManagement API to delete tenants.");

    }
}
