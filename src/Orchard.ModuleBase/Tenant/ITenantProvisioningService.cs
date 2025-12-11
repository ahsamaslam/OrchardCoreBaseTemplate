namespace Orchard.ModuleBase.Tenant
{
    public record ProvisionOptions(bool RunMigrations = true, bool RunRecipe = true, bool SeedAdmin = true, bool RunSynchronously = false);
    public record ProvisioningStatus(string TenantId, string TaskId, string State, string? Message, DateTimeOffset UpdatedUtc);

    public interface ITenantProvisioningService
    {
        Task<string> EnqueueProvisionAsync(TenantContext tenant, ProvisionOptions options, CancellationToken ct = default);
        Task<ProvisioningStatus?> GetStatusAsync(string taskId);
        Task<ProvisioningStatus?> GetStatusByTenantAsync(string tenantId);
    }
}