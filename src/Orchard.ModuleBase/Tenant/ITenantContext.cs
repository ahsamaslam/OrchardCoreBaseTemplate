namespace Orchard.ModuleBase.Tenant
{
    public interface ITenantContext
    {
        string TenantId { get; }
        string TenantName { get; }
        string? ConnectionString { get; }
        IDictionary<string, string> Settings { get; }
    }
}
