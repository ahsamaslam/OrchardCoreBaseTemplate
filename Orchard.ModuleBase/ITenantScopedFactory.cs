namespace Orchard.ModuleBase
{
    public interface ITenantScopedFactory<T>
    {
        T Create(ITenantContext tenantContext);

    }
}
