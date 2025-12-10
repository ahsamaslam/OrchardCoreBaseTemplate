namespace Orchard.Setup.Services
{
    public interface ISetupStateService
    {
        /// <summary>
        /// Returns true if setup is required (first-run).
        /// </summary>
        Task<bool> IsSetupRequiredAsync();

        /// <summary>
        /// Mark setup completed.
        /// </summary>
        Task MarkSetupCompletedAsync();
    }
}
