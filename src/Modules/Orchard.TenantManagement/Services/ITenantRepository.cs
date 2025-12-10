using Orchard.TenantManagement.Models;
namespace Orchard.TenantManagement.Services
{
    public interface ITenantRepository
    {
        Task AddAsync(TenantInfo tenant);
        Task<TenantInfo?> FindByIdAsync(string tenantId);
        Task<TenantInfo?> FindByHostAsync(string host);
        Task<IEnumerable<TenantInfo>> ListAsync();
        Task<bool> RemoveAsync(string tenantId);
    }
}
