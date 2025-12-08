
namespace Orchard.ModuleBase
{
    public interface ITenantContext
    {
        string TenantId { get; }
        string TenantName { get; }
        string? ConnectionString { get; }
        IReadOnlyDictionary<string, string> Settings { get; }
    }
}
