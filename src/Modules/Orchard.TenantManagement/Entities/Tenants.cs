using Orchard.TenantManagement.Models;

namespace Orchard.TenantManagement.Entities
{
    public class Tenants
    {
        public string TenantId { get; set; } = null!;
        public string TenantName { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;
        public string? Hosts { get; set; }
        public string? SettingsJson { get; set; }
        public DateTime CreatedUtc { get; set; }
        public bool IsActive { get; set; }

        public TenantInfo ToModel() =>
            new TenantInfo
            {
                TenantId = TenantId,
                TenantName = TenantName,
                ConnectionString = ConnectionString,
                Hosts = Hosts,
                SettingsJson = SettingsJson,
                CreatedUtc = CreatedUtc,
                IsActive = IsActive
            };
    }
}
