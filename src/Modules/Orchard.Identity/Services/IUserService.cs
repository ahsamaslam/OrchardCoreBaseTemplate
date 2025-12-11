// src/Modules/Orchard.Identity/Services/IUserService.cs
using Orchard.Identity.Models;
using Orchard.ModuleBase.Tenant;
using System;
using System.Threading.Tasks;

namespace Orchard.Identity.Services
{
    public interface IUserService
    {
        Task CreateAsync(ITenantContext tenant, IdentityUser user);
        Task<IdentityUser?> FindByIdAsync(ITenantContext tenant, Guid id);
        Task<IdentityUser?> FindByNameAsync(ITenantContext tenant, string normalizedUserName);
        Task UpdateAsync(ITenantContext tenant, IdentityUser user);
        Task DeleteAsync(ITenantContext tenant, Guid id);

        // role helpers
        Task AddToRoleAsync(ITenantContext tenant, Guid userId, string normalizedRoleName);
        Task RemoveFromRoleAsync(ITenantContext tenant, Guid userId, string normalizedRoleName);
        Task<bool> IsInRoleAsync(ITenantContext tenant, Guid userId, string normalizedRoleName);
    }
}
