// Host/Infrastructure/Migrations/ResilientPostStartupMigrationsHostedService.cs
using Microsoft.Extensions.Options;
using Orchard.ModuleBase;
using System.Reflection;

public class PostStartupMigrationsHostedService : IHostedService
{
    private readonly ILogger<PostStartupMigrationsHostedService> _logger;
    private readonly IMigrationRunnerService _runner;
    private readonly ModuleRegistryOptions _moduleRegistry;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IConfiguration _configuration;
    private readonly int _maxWaitSeconds = 10; // configurable

    public PostStartupMigrationsHostedService(
        ILogger<PostStartupMigrationsHostedService> logger,
        IMigrationRunnerService runner,
        IOptions<ModuleRegistryOptions> moduleRegistry,
        IHostApplicationLifetime lifetime,
        IConfiguration configuration)
    {
        _logger = logger;
        _runner = runner;
        _moduleRegistry = moduleRegistry.Value;
        _lifetime = lifetime;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    await TryRunHostMigrationsWithRetryAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while running resilient post-startup host migrations");
                }
            }, cancellationToken);
        });

        return Task.CompletedTask;
    }

    private async Task TryRunHostMigrationsWithRetryAsync(CancellationToken ct)
    {
        var hostConn = _configuration.GetConnectionString("Host");
        if (string.IsNullOrWhiteSpace(hostConn))
        {
            _logger.LogWarning("ConnectionStrings:Host not configured; skipping host migrations.");
            return;
        }

        // Wait for at most _maxWaitSeconds, polling each second for registrations
        var waited = 0;
        while (waited <= _maxWaitSeconds)
        {
            if (_moduleRegistry.MigrationAssemblies != null && _moduleRegistry.MigrationAssemblies.Count > 0)
                break;

            _logger.LogWarning("No module migration assemblies registered yet. Waiting... {Waited}s", waited);
            await Task.Delay(1000, ct);
            waited++;
        }

        // If still empty, log diagnostics and bail (but do not crash host)
        if (_moduleRegistry.MigrationAssemblies == null || _moduleRegistry.MigrationAssemblies.Count == 0)
        {
            _logger.LogError("ModuleRegistryOptions.MigrationAssemblies still empty after {MaxWait}s. Dumping diagnostics...", _maxWaitSeconds);

            try
            {
                var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .OrderBy(a => a.FullName)
                    .Select(a => new { a.FullName, Location = SafeGetLocation(a) })
                    .ToList();

                _logger.LogError("AppDomain assemblies ({Count}):\n{List}",
                    domainAssemblies.Count,
                    string.Join("\n", domainAssemblies.Select(d => $"{d.FullName} | {d.Location}")));

                _logger.LogError("ModuleRegistryOptions currently has {Count} assemblies (should be >0).", _moduleRegistry.MigrationAssemblies?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enumerate AppDomain assemblies");
            }

            // Do not throw; allow host to continue. Admin can call manual migration endpoint.
            return;
        }

        _logger.LogInformation("Found {Count} module migration assemblies. Running host migrations.", _moduleRegistry.MigrationAssemblies.Count);

        foreach (var asm in _moduleRegistry.MigrationAssemblies)
        {
            try
            {
                _logger.LogInformation("Running host migrations from assembly {Assembly}", asm.FullName);
                await _runner.RunMigrationsAsync(hostConn, asm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run migrations from {Assembly}", asm.FullName);
            }
        }

        _logger.LogInformation("Resilient post-startup host migrations completed.");
    }

    private static string SafeGetLocation(Assembly a)
    {
        try { return a.Location; }
        catch { return "(no location)"; }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
