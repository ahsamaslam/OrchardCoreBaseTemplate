
using Microsoft.AspNetCore.Authentication;
using Orchard.ModuleBase.Tenant;

namespace Orchard.ModuleBase
{
    public interface IExternalAuthProvider
    {
        string ProviderName { get; }
        bool IsEnabled(ITenantContext tenant);
        Task ConfigureAsync(AuthenticationBuilder builder, ITenantContext tenantContext);
    }
}
