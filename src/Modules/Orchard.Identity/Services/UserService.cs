// src/Modules/Orchard.Identity/Services/UserService.cs
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using Orchard.Identity.Models;
using Orchard.ModuleBase;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly ITenantScopedFactory<DataConnection> _connectionFactory;

        public UserService(ITenantScopedFactory<DataConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task CreateAsync(ITenantContext tenant, IdentityUser user)
        {
            using var db = _connectionFactory.Create(tenant);
            await db.InsertAsync(user);
        }

        public async Task<IdentityUser?> FindByIdAsync(ITenantContext tenant, Guid id)
        {
            using var db = _connectionFactory.Create(tenant);
            return await db.GetTable<IdentityUser>().FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IdentityUser?> FindByNameAsync(ITenantContext tenant, string normalizedUserName)
        {
            using var db = _connectionFactory.Create(tenant);
            return await db.GetTable<IdentityUser>().FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName);
        }

        public async Task UpdateAsync(ITenantContext tenant, IdentityUser user)
        {
            using var db = _connectionFactory.Create(tenant);
            await db.UpdateAsync(user);
        }

        public async Task DeleteAsync(ITenantContext tenant, Guid id)
        {
            using var db = _connectionFactory.Create(tenant);
            await db.GetTable<IdentityUser>().Where(u => u.Id == id).DeleteAsync();
        }

        // Roles helpers: find role id by normalized name, insert/delete join table entries
        public async Task AddToRoleAsync(ITenantContext tenant, Guid userId, string normalizedRoleName)
        {
            using var db = _connectionFactory.Create(tenant);

            var role = await db.GetTable<Role>().FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName);
            if (role == null)
            {
                // optional: throw or create role
                role = new Role { Id = Guid.NewGuid(), Name = normalizedRoleName, NormalizedName = normalizedRoleName };
                await db.InsertAsync(role);
            }

            // avoid duplicates
            var exists = await db.GetTable<UserRole>().AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
            if (!exists)
            {
                await db.InsertAsync(new UserRole { UserId = userId, RoleId = role.Id });
            }
        }

        public async Task RemoveFromRoleAsync(ITenantContext tenant, Guid userId, string normalizedRoleName)
        {
            using var db = _connectionFactory.Create(tenant);
            var role = await db.GetTable<Role>().FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName);
            if (role == null) return;
            await db.GetTable<UserRole>().Where(ur => ur.UserId == userId && ur.RoleId == role.Id).DeleteAsync();
        }

        public async Task<bool> IsInRoleAsync(ITenantContext tenant, Guid userId, string normalizedRoleName)
        {
            using var db = _connectionFactory.Create(tenant);
            var role = await db.GetTable<Role>().FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName);
            if (role == null) return false;
            return await db.GetTable<UserRole>().AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
        }
    }
}
