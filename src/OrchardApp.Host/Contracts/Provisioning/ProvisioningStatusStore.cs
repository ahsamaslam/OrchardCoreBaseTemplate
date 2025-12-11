namespace OrchardApp.Host.Contracts.Provisioning
{
    using Orchard.ModuleBase;
    using Orchard.ModuleBase.Tenant;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    // Uses the ISettingsStore abstraction to persist a JSON blob under a single settings key.
    public class ProvisioningStatusStore : IProvisioningStatusStore
    {
        private const string SettingsKey = "Provisioning:Status";
        private readonly ISettingsStore _settingsStore;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProvisioningStatusStore(ISettingsStore settingsStore)
        {
            _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                // you can add converters here if needed
            };
        }

        public async Task<ProvisioningStatus?> GetStatusAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));

            var settings = await _settingsStore.LoadSettingsAsync(tenantId);
            if (settings == null || !settings.TryGetValue(SettingsKey, out var json) || string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                var status = JsonSerializer.Deserialize<ProvisioningStatus>(json, _jsonOptions);
                return status;
            }
            catch
            {
                // Corrupt data — swallow and return null (or you could throw to force manual recovery)
                return null;
            }
        }

        public async Task SaveStatusAsync(string tenantId, ProvisioningStatus status, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
            if (status == null) throw new ArgumentNullException(nameof(status));

            var settings = await _settingsStore.LoadSettingsAsync(tenantId) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            settings[SettingsKey] = JsonSerializer.Serialize(status, _jsonOptions);

            await _settingsStore.SaveSettingsAsync(tenantId, settings);
        }

        public async Task ClearStatusAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));

            var settings = await _settingsStore.LoadSettingsAsync(tenantId);
            if (settings == null || !settings.ContainsKey(SettingsKey)) return;

            settings.Remove(SettingsKey);
            await _settingsStore.SaveSettingsAsync(tenantId, settings);
        }
    }
}
