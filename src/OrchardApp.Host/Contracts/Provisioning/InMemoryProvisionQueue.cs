using Orchard.ModuleBase;
using Orchard.ModuleBase.Tenant;
using System.Collections.Concurrent;

namespace OrchardApp.Host.Contracts.Provisioning
{
    public class ProvisionQueueItem { public string Id { get; set; } = Guid.NewGuid().ToString("n"); public TenantContext Tenant { get; set; } = null!; public ProvisionOptions Options { get; set; } = new(); }
    public interface IProvisionQueue { ValueTask EnqueueAsync(ProvisionQueueItem item); ValueTask<ProvisionQueueItem?> DequeueAsync(CancellationToken ct); }

    public class InMemoryProvisionQueue : IProvisionQueue
    {
        private readonly SemaphoreSlim _signal = new(0);
        private readonly ConcurrentQueue<ProvisionQueueItem> _q = new();
        public ValueTask EnqueueAsync(ProvisionQueueItem item) { _q.Enqueue(item); _signal.Release(); return new ValueTask(); }
        public async ValueTask<ProvisionQueueItem?> DequeueAsync(CancellationToken ct) { await _signal.WaitAsync(ct); _q.TryDequeue(out var it); return it; }
    }
}
