namespace Orchard.TenantManagement.Models
{
    public class TenantInfo
    {
        public string TenantId { get; set; } = null!;
        public string TenantName { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;
        public string? Hosts { get; set; }
        public string? SettingsJson { get; set; }
        public DateTime CreatedUtc { get; set; }
        public bool IsActive { get; set; } = true;

        public IDictionary<string, string> GetSettingsDictionary()
        {
            if (string.IsNullOrWhiteSpace(SettingsJson)) return new Dictionary<string, string>();
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(SettingsJson!) ?? new Dictionary<string, string>();
            }
            catch { return new Dictionary<string, string>(); }
        }
    }
}
