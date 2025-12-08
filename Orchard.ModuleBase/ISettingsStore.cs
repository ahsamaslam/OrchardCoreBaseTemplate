namespace Orchard.ModuleBase
{
    public interface ISettingsStore
    {
        Task<T?> GetAsync<T>(string key, string? tenantId = null);
        Task SetAsync<T>(string key, T value, string? tenantId = null);
        Task RemoveAsync(string key, string? tenantId = null);
    }
}
