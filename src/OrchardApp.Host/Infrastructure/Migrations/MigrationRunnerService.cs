// Host/Infrastructure/Migrations/MigrationRunnerService.cs
using FluentMigrator.Runner;
using Orchard.ModuleBase;
using Orchard.ModuleBase.Tenant;
using System.Reflection;

public class MigrationRunnerService : IMigrationRunnerService
{
    private readonly ModuleRegistryOptions _moduleRegistry;
    private readonly ILogger<MigrationRunnerService> _logger;

    public MigrationRunnerService(Microsoft.Extensions.Options.IOptions<ModuleRegistryOptions> moduleRegistry,
                                  ILogger<MigrationRunnerService> logger)
    {
        _moduleRegistry = moduleRegistry.Value;
        _logger = logger;
    }

    public async Task RunMigrationsAsync(string connectionString, Assembly assembly)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is null or empty", nameof(connectionString));

        _logger.LogInformation("Preparing FluentMigrator runner for assembly {Assembly} against connection {Conn}", assembly.FullName, connectionString);

        // Build a service provider for FluentMigrator that scans ONLY the supplied assembly
        var services = new ServiceCollection()
            .AddLogging(lb => lb.AddConsole())
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                // NOTE: choose provider you need; this example uses SQL Server.
                // If you require multi-provider, you can add branch logic or configure per-tenant provider.
                rb.AddSqlServer()
                  .WithGlobalConnectionString(connectionString)
                  .ScanIn(assembly).For.Migrations();
            })
            .BuildServiceProvider(false);

        // Log what migrations were found in the assembly (best-effort)
        try
        {
            var migrationTypes = assembly.GetTypes()
                                        .Where(t => t.GetCustomAttributes(typeof(FluentMigrator.MigrationAttribute), inherit: false).Any())
                                        .ToArray();

            if (!migrationTypes.Any())
            {
                _logger.LogWarning("No migration types found in assembly {Assembly}.", assembly.FullName);
            }
            else
            {
                _logger.LogInformation("Found {Count} migrations in assembly {Assembly}: {Migs}",
                    migrationTypes.Length, assembly.FullName,
                    string.Join(", ", migrationTypes.Select(t => t.Name)));
            }
        }
        catch (ReflectionTypeLoadException rtle)
        {
            _logger.LogWarning(rtle, "ReflectionTypeLoadException while scanning assembly {Assembly}", assembly.FullName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exception while enumerating migrations in {Assembly}", assembly.FullName);
        }

        using var scope = services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        try
        {
            await Task.Run(() =>
            {
                _logger.LogInformation("Starting MigrationRunner.MigrateUp() for {Asm}", assembly.GetName().Name);
                runner.MigrateUp();
                _logger.LogInformation("MigrationRunner completed for {Asm}", assembly.GetName().Name);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FluentMigrator failed for assembly {Assembly}", assembly.FullName);
            throw;
        }

        //return Task.CompletedTask;
    }

    public async Task RunMigrationsForTenantAsync(ITenantContext tenantContext)
    {
        if (tenantContext == null) throw new ArgumentNullException(nameof(tenantContext));
        if (string.IsNullOrEmpty(tenantContext.ConnectionString)) throw new InvalidOperationException("Tenant has no connection string.");

        _logger.LogInformation("Running tenant migrations for tenant {Tenant}", tenantContext.TenantId);

        foreach (var asm in _moduleRegistry.MigrationAssemblies)
        {
            await RunMigrationsAsync(tenantContext.ConnectionString, asm);
        }
    }
}
