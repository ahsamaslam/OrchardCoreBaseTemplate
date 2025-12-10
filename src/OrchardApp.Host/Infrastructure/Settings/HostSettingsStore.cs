using Orchard.ModuleBase;
using System.Collections.Concurrent;

namespace OrchardApp.Host.Infrastructure.Settings
{
    /// <summary>
    /// Simple in-memory settings store for per-tenant settings.
    /// Replace with EF Core / Linq2DB / Redis / SQL table later.
    /// </summary>
    public class HostSettingsStore : ISettingsStore
    {
        // tenantId -> (key -> value)
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _store
            = new(StringComparer.OrdinalIgnoreCase);

        public Task<IDictionary<string, string>> LoadSettingsAsync(string tenantId)
        {
            if (tenantId == null)
                throw new ArgumentNullException(nameof(tenantId));

            var tenantSettings = _store.GetOrAdd(tenantId, _ => new ConcurrentDictionary<string, string>());

            // return a copy to avoid external mutation
            var result = new Dictionary<string, string>(tenantSettings, StringComparer.OrdinalIgnoreCase);

            return Task.FromResult<IDictionary<string, string>>(result);
        }

        public Task SaveSettingsAsync(string tenantId, IDictionary<string, string> settings)
        {
            if (tenantId == null)
                throw new ArgumentNullException(nameof(tenantId));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var tenantSettings = _store.GetOrAdd(tenantId, _ => new ConcurrentDictionary<string, string>());

            // Replace all settings for tenant
            tenantSettings.Clear();

            foreach (var kv in settings)
            {
                tenantSettings[kv.Key] = kv.Value;
            }

            return Task.CompletedTask;
        }
    }
}
