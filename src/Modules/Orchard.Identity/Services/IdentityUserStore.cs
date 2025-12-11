// src/Modules/Orchard.Identity/Services/IdentityUserStore.cs
using Microsoft.AspNetCore.Identity;
using AppIdentityUser = Orchard.Identity.Models.IdentityUser; // alias to avoid IdentityUser collision
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orchard.ModuleBase.Tenant;

namespace Orchard.Identity.Services
{
    public class IdentityUserStore :
        IUserStore<AppIdentityUser>,
        IUserPasswordStore<AppIdentityUser>,
        IUserRoleStore<AppIdentityUser>
    {
        private readonly IUserService _userService;
        private readonly ITenantContext _tenantContext; // obtain tenant context per request

        public IdentityUserStore(IUserService userService, ITenantContext tenantContext)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public void Dispose() { /* nothing to dispose */ }

        public async Task<IdentityResult> CreateAsync(AppIdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _userService.CreateAsync(_tenantContext, user);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(AppIdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _userService.DeleteAsync(_tenantContext, user.Id);
            return IdentityResult.Success;
        }

        public async Task<AppIdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Guid.TryParse(userId, out var id)) return null;
            return await _userService.FindByIdAsync(_tenantContext, id);
        }

        public async Task<AppIdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _userService.FindByNameAsync(_tenantContext, normalizedUserName);
        }

        public Task<string?> GetNormalizedUserNameAsync(AppIdentityUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.NormalizedUserName);

        public Task<string> GetUserIdAsync(AppIdentityUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Id.ToString());

        public Task<string?> GetUserNameAsync(AppIdentityUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.UserName);

        public Task SetNormalizedUserNameAsync(AppIdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(AppIdentityUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(AppIdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _userService.UpdateAsync(_tenantContext, user);
            return IdentityResult.Success;
        }

        // Password store
        public Task SetPasswordHashAsync(AppIdentityUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string?> GetPasswordHashAsync(AppIdentityUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.PasswordHash);

        public Task<bool> HasPasswordAsync(AppIdentityUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.PasswordHash != null);

        // Role store methods
        public async Task AddToRoleAsync(AppIdentityUser user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _userService.AddToRoleAsync(_tenantContext, user.Id, normalizedRoleName);
        }

        public async Task RemoveFromRoleAsync(AppIdentityUser user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _userService.RemoveFromRoleAsync(_tenantContext, user.Id, normalizedRoleName);
        }

        public async Task<IList<string>> GetRolesAsync(AppIdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Implement efficient roles retrieval in IUserService if possible
            // For now, return roles by querying via IUserService (you can add a GetRolesForUserAsync)
            // We'll assume there's a helper (not implemented earlier) — fallback to empty list:
            return new List<string>();
        }

        public Task<bool> IsInRoleAsync(AppIdentityUser user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _userService.IsInRoleAsync(_tenantContext, user.Id, normalizedRoleName);
        }

        public Task<IList<AppIdentityUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("GetUsersInRoleAsync not implemented - add to IUserService if needed");
        }
    }
}
