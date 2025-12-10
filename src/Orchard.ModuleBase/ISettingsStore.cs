namespace Orchard.ModuleBase
{
    public interface ISettingsStore
    {
        Task<IDictionary<string, string>> LoadSettingsAsync(string tenantId);
        Task SaveSettingsAsync(string tenantId, IDictionary<string, string> settings);
    }
}
