namespace Orchard.ModuleBase.Tenant
{
    public interface ITenantScopedFactory<T>
    {
        T Create(ITenantContext tenantContext);

    }
}
