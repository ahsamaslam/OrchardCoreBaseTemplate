// Host/Tenants/InMemoryTenantStore.cs
using Orchard.ModuleBase;
using System.Collections.Concurrent;

/// <summary>
/// Simple in-memory tenant store for testing/dev.
/// Keeps tenants indexed by tenant id and by host names (one or many).
/// </summary>
public class InMemoryTenantStore : ITenantStore
{
    // store tenant by id
    private readonly ConcurrentDictionary<string, ITenantContext> _byId = new(StringComparer.OrdinalIgnoreCase);

    // store tenant by host (e.g. "tenant1.example.com")
    private readonly ConcurrentDictionary<string, ITenantContext> _byHost = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds (or replaces) a tenant. If tenant.Settings contains a "Hosts" entry (comma-separated),
    /// those hostnames will be registered as well. Otherwise the tenant.TenantName is used as host key.
    /// </summary>
    public Task AddTenantAsync(ITenantContext tenant)
    {
        if (tenant == null) throw new ArgumentNullException(nameof(tenant));

        // Add/replace by tenant id
        _byId[tenant.TenantId] = tenant;

        // Determine hostnames to register for this tenant:
        // - If settings contains "Hosts" (comma separated) use them
        // - Else if tenant.TenantName looks like a hostname use that
        // - Otherwise do not register hosts (caller may register manually)
        IEnumerable<string> hosts = Enumerable.Empty<string>();

        if (tenant.Settings != null && tenant.Settings.TryGetValue("Hosts", out var hs) && !string.IsNullOrWhiteSpace(hs))
        {
            hosts = hs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(h => h.Trim())
                      .Where(h => !string.IsNullOrEmpty(h));
        }
        else if (!string.IsNullOrWhiteSpace(tenant.TenantName))
        {
            hosts = new[] { tenant.TenantName };
        }

        foreach (var host in hosts)
        {
            _byHost[host] = tenant;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Find tenant by host (e.g. request.Host.Host).
    /// </summary>
    public Task<ITenantContext?> FindByHostAsync(string host)
    {
        if (string.IsNullOrWhiteSpace(host)) return Task.FromResult<ITenantContext?>(null);

        _byHost.TryGetValue(host, out var tenant);
        return Task.FromResult(tenant);
    }

    /// <summary>
    /// Optional: find by tenant id
    /// </summary>
    public Task<ITenantContext?> FindByIdAsync(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return Task.FromResult<ITenantContext?>(null);

        _byId.TryGetValue(tenantId, out var tenant);
        return Task.FromResult(tenant);
    }

    /// <summary>
    /// List all tenants.
    /// </summary>
    public Task<IEnumerable<ITenantContext>> ListAsync()
    {
        return Task.FromResult<IEnumerable<ITenantContext>>(_byId.Values.ToList());
    }

    /// <summary>
    /// Remove tenant by id (and any registered hosts).
    /// </summary>
    public Task<bool> RemoveTenantAsync(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return Task.FromResult(false);

        if (!_byId.TryRemove(tenantId, out var removed)) return Task.FromResult(false);

        // Remove any registered hosts that pointed to this tenant
        var keysToRemove = _byHost.Where(kv => kv.Value.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase))
                                  .Select(kv => kv.Key)
                                  .ToList();

        foreach (var key in keysToRemove)
            _byHost.TryRemove(key, out _);

        return Task.FromResult(true);
    }
}
