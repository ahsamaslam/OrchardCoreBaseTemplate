
namespace Orchard.ModuleBase
{
    public interface ITenantContext
    {
        string TenantId { get; }
        string TenantName { get; }
        string? ConnectionString { get; }
        IDictionary<string, string> Settings { get; }
    }
}
