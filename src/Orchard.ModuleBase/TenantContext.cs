namespace Orchard.ModuleBase
{
    public class TenantContext : ITenantContext
    {
        public TenantContext(
            string tenantId,
            string tenantName,
            string? connectionString,
            IDictionary<string, string> settings)
        {
            TenantId = tenantId;
            TenantName = tenantName;
            ConnectionString = connectionString;
            Settings = settings;
        }

        public string TenantId { get; }
        public string TenantName { get; }
        public string? ConnectionString { get; }
        public IDictionary<string, string> Settings { get; }
    }
}
