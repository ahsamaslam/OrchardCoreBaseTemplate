namespace Orchard.ModuleBase
{
    public class TenantContext : ITenantContext
    {
        public TenantContext(
            string tenantId,
            string tenantName,
            string? connectionString,
            IReadOnlyDictionary<string, string> settings)
        {
            TenantId = tenantId;
            TenantName = tenantName;
            ConnectionString = connectionString;
            Settings = settings;
        }

        public string TenantId { get; }
        public string TenantName { get; }
        public string? ConnectionString { get; }
        public IReadOnlyDictionary<string, string> Settings { get; }
    }
}
