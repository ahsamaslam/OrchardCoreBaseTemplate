// Program.cs (Host)
// ------------------------------------------------------

using LinqToDB.Data;
using Microsoft.Extensions.Options;
using Orchard.ModuleBase;
using Orchard.ModuleBase.Tenant;
using Orchard.TenantManagement.Services;
using OrchardApp.Host;
using OrchardApp.Host.Contracts.Provisioning;
using OrchardApp.Host.Database;
using OrchardApp.Host.Infrastructure.Settings;
using OrchardApp.Host.Tenants;
using System.Reflection;
using System.Runtime.Loader;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// -----------------------------------------------------------------------------
// 0. Register module options (required BEFORE module bootstrapping)
// -----------------------------------------------------------------------------
builder.Services.Configure<ModuleRegistryOptions>(o => { });

// -----------------------------------------------------------------------------
// 1. Register global services needed BEFORE module startup execution
// -----------------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantContext>(sp =>
{
    var http = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    if (http?.Items.TryGetValue("CurrentTenant", out var t) == true && t is ITenantContext tc)
        return tc;

    throw new InvalidOperationException("Tenant not resolved for the current request.");
});

// Linq2DB tenant factory
builder.Services.AddSingleton<ITenantScopedFactory<DataConnection>, TenantLinq2DbFactory>();

// Host-level settings store
builder.Services.AddSingleton<ISettingsStore, HostSettingsStore>();



// ============================================================================
// 2. MODULE BOOTSTRAP: Load modules + execute ConfigureModuleServices()
//    BEFORE we register things like ITenantStore or run any migrations.
// ============================================================================
BootstrapModuleServicesVerbose(builder.Services);

// ============================================================================
// 3. Register services that depend on module-registered services
// ============================================================================
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ITenantStore, HostTenantStoreAdapter>();

// Migration runner (Host-side implementation)
builder.Services.AddSingleton<IMigrationRunnerService, MigrationRunnerService>();


builder.Services.AddSingleton<IProvisionQueue, InMemoryProvisionQueue>();
builder.Services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
builder.Services.AddSingleton<ITenantDatabaseCreator, SqlServerTenantDatabaseCreator>();
builder.Services.AddHostedService<TenantProvisioningHostedService>();
builder.Services.AddSingleton<IProvisioningStatusStore, ProvisioningStatusStore>();


// Orchard
builder.Services.AddOrchardCore().WithTenants();

var app = builder.Build();
app.UseRouting();

//if (app.Environment.IsDevelopment())
//{
//    using var scope = app.Services.CreateScope();
//    var tenantStore = scope.ServiceProvider.GetRequiredService<ITenantStore>();
//    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

//    // Read connection strings from appsettings (fall back to Host if Tenant1 is missing)
//    var hostConn = config.GetConnectionString("Host");
//    var tenant1Conn = config.GetConnectionString("Tenant1") ?? hostConn;

//    // Sanity checks and logging
//    var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("TenantSeed");
//    logger?.LogInformation("Seeding tenants (Dev). HostConn: {HostConnPresent}, Tenant1Conn: {Tenant1ConnPresent}",
//        !string.IsNullOrEmpty(hostConn), !string.IsNullOrEmpty(tenant1Conn));

//    // Tenant #1: uses host connection (example: admin / host DB)
//    var tenantA = new TenantContext(
//        tenantId: "t1",
//        tenantName: "tenant.local",                 // used as host key by default
//        connectionString: hostConn,
//        settings: new Dictionary<string, string>
//        {
//            // register hostnames to resolve this tenant in TenantResolutionMiddleware
//            // For local testing, map these names in your hosts file (e.g. 127.0.0.1 tenant.local)
//            { "Hosts", "tenant.local,localhost,127.0.0.1" }
//        });

//    await tenantStore.AddTenantAsync(tenantA);
//    logger?.LogInformation("Added tenant {Id} -> hosts {Hosts}", tenantA.TenantId, tenantA.Settings?["Hosts"]);

//    // Tenant #2: uses Tenant1 connection string (separate DB)
//    var tenantB = new TenantContext(
//        tenantId: "t2",
//        tenantName: "tenant.tenant1",
//        connectionString: tenant1Conn,
//        settings: new Dictionary<string, string>
//        {
//            // Example hosts — add mapping in hosts file for local testing
//            { "Hosts", "tenant.tenant1,tenant1.local" }
//        });

//    await tenantStore.AddTenantAsync(tenantB);
//    logger?.LogInformation("Added tenant {Id} -> hosts {Hosts}", tenantB.TenantId, tenantB.Settings?["Hosts"]);
//}

// ============================================================================
// 4. Run Host migrations BEFORE ANY middleware that touches ITenantStore
// ============================================================================
//await RunHostMigrationsAsync(app);

// ============================================================================
// 5. Register middleware AFTER migrations
// ============================================================================
app.UseMiddleware<TenantResolutionMiddleware>();

// ---------------- Debug endpoints -------------------
app.MapGet("/_diag-assemblies", () =>
{
    var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => !a.IsDynamic)
                        .Select(a => new { Name = a.GetName().Name, FullName = a.FullName, Location = a.Location })
                        .ToList();

    var dupNames = assemblies
        .GroupBy(a => a.Name)
        .Where(g => g.Count() > 1)
        .ToDictionary(g => g.Key, g => g.Select(x => x.Location).ToList());

    return Results.Json(new { Count = assemblies.Count, Assemblies = assemblies, Duplicates = dupNames });
});

// dump ApplicationParts and controller/action descriptors
//app.MapGet("/_diag-mvc", (IServiceProvider sp) =>
//{
//    var apm = sp.GetRequiredService<ApplicationPartManager>();
//    var parts = apm.ApplicationParts.Select(p => new { p.Name, Kind = p.GetType().Name }).ToList();

//    var adp = sp.GetService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>();
//    var actions = adp?.ActionDescriptors.Items
//        .Select(a => new { a.DisplayName, RouteValues = a.RouteValues })
//        .ToList() ?? new List<object>();

//    return Results.Json(new { ApplicationParts = parts, ActionCount = actions.Count, Actions = actions.Take(250) }); // limit to 250
//});
app.MapGet("/_migration-debug", (IOptions<ModuleRegistryOptions> opts) =>
{
    var assemblies = opts.Value.MigrationAssemblies.Select(a => a.FullName).ToList();
    return Results.Json(new { Count = assemblies.Count, Assemblies = assemblies });
});

app.MapPost("/_run-host-migrations", async (IMigrationRunnerService runner, IConfiguration cfg, IOptions<ModuleRegistryOptions> opts) =>
{
    var hostConn = cfg.GetConnectionString("Host");
    if (string.IsNullOrEmpty(hostConn))
        return Results.Problem("Host connection string not configured.");

    var assemblies = opts.Value.MigrationAssemblies;
    var results = new List<object>();

    foreach (var asm in assemblies)
    {
        try
        {
            await runner.RunMigrationsAsync(hostConn, asm);
            results.Add(new { Assembly = asm.GetName().Name, Status = "OK" });
        }
        catch (Exception ex)
        {
            // Return full exception text to client for debugging (dev only)
            return Results.Problem(detail: ex.ToString(), title: $"Migration failed for {asm.GetName().Name}");
        }
    }

    return Results.Ok(new { Ran = assemblies.Count, Results = results });
});
// Orchard pipeline
app.UseOrchardCore();

app.Lifetime.ApplicationStarted.Register(() =>
{
    // Run async but don't block ApplicationStarted registration
    _ = Task.Run(async () =>
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("PostStart");
            logger.LogInformation("ApplicationStarted callback executing post-start tasks...");

            var opts = scope.ServiceProvider.GetRequiredService<IOptions<ModuleRegistryOptions>>();
            logger.LogInformation("Migration assemblies found: {Count}", opts.Value.MigrationAssemblies.Count);

            if (opts.Value.MigrationAssemblies.Count > 0)
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunnerService>();
                var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var hostConn = cfg.GetConnectionString("Host") ?? throw new InvalidOperationException("Host connection string missing");

                foreach (var asm in opts.Value.MigrationAssemblies)
                {
                    try
                    {
                        logger.LogInformation("Running host migrations for assembly {Name}", asm.GetName().Name);
                        await runner.RunMigrationsAsync(hostConn, asm);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Migration failed for {Name}", asm.GetName().Name);
                        // do not rethrow — failing here would crash the host at startup
                    }
                }
            }

            // call provisioning / recipe executor as needed
            // var provisioning = scope.ServiceProvider.GetRequiredService<ITenantProvisioningService>();
            // await provisioning.RunPendingProvisioningAsync();

            logger.LogInformation("ApplicationStarted callback finished.");
        }
        catch (Exception ex)
        {
            // Last-resort logging — do not crash host
            var lf = app.Services.GetService<ILoggerFactory>();
            lf?.CreateLogger("PostStart")?.LogError(ex, "ApplicationStarted callback fatal error");
        }
    });
});
app.Run();


// ============================================================================
// BOOTSTRAP MODULES — loads assemblies and executes ConfigureModuleServices()
// ============================================================================
void BootstrapModuleServicesVerbose(IServiceCollection services)
{
    var moduleStartupType = typeof(ModuleStartupBase);

    Console.WriteLine("[Bootstrap] AppContext.BaseDirectory: " + AppContext.BaseDirectory);

    // 1) list Modules folder files (helpful)
    var modulesDir = Path.Combine(AppContext.BaseDirectory, "Modules");
    Console.WriteLine("[Bootstrap] Modules folder present: " + Directory.Exists(modulesDir));
    if (Directory.Exists(modulesDir))
    {
        foreach (var f in Directory.GetFiles(modulesDir, "*.dll", SearchOption.AllDirectories))
            Console.WriteLine("[Bootstrap] Module file: " + f);
    }

    // 2) list bin folder dlls (helpful)
    var binDir = AppContext.BaseDirectory;
    foreach (var f in Directory.GetFiles(binDir, "*.dll", SearchOption.TopDirectoryOnly))
        Console.WriteLine("[Bootstrap] Bin DLL: " + f);

    // 3) attempt to explicitly load Orchard.Identity.dll if present in Modules or bin
    string[] candidatePaths = new[]
    {
        Path.Combine(modulesDir, "Orchard.Identity.dll"),
        Path.Combine(binDir, "Orchard.Identity.dll"),
        Path.Combine(binDir, "Orchard.Identity", "Orchard.Identity.dll")
    };

    foreach (var p in candidatePaths.Distinct())
    {
        if (File.Exists(p))
        {
            try
            {
                var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(p));
                Console.WriteLine($"[Bootstrap] Explicitly loaded assembly from {p}: {asm.GetName().Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Bootstrap] Failed to load {p}: {ex}");
            }
        }
    }

    // 4) gather candidate assemblies (already-loaded + modules folder)
    var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToList();

    if (Directory.Exists(modulesDir))
    {
        foreach (var dll in Directory.GetFiles(modulesDir, "*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(dll));
                if (!assemblies.Any(a => a.GetName().Name == asm.GetName().Name))
                    assemblies.Add(asm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Bootstrap] Could not load {dll}: {ex.Message}");
            }
        }
    }

    Console.WriteLine("[Bootstrap] Assemblies considered:");
    foreach (var a in assemblies.OrderBy(x => x.GetName().Name))
        Console.WriteLine($"  - {a.GetName().Name} @ {a.Location}");

    // 5) discover ModuleStartupBase types, but handle ReflectionTypeLoadException
    var startupTypes = new List<Type>();
    foreach (var asm in assemblies)
    {
        try
        {
            var types = asm.GetTypes()
                           .Where(t => moduleStartupType.IsAssignableFrom(t) && !t.IsAbstract)
                           .ToList();

            foreach (var t in types)
            {
                startupTypes.Add(t);
                Console.WriteLine($"[Bootstrap] Found Startup type: {t.FullName} in {asm.GetName().Name}");
            }
        }
        catch (ReflectionTypeLoadException rtle)
        {
            Console.WriteLine($"[Bootstrap] ReflectionTypeLoadException in assembly {asm.GetName().Name}:");
            foreach (var le in rtle.LoaderExceptions)
                Console.WriteLine("   LoaderException: " + le.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Bootstrap] GetTypes failed for {asm.GetName().Name}: {ex.Message}");
        }
    }

    // 6) instantiate and invoke ConfigureModuleServices with logging of exceptions
    using var tempSp = services.BuildServiceProvider();

    foreach (var st in startupTypes)
    {
        Console.WriteLine($"[Bootstrap] Instantiating {st.FullName}");
        try
        {
            var instance = ActivatorUtilities.CreateInstance(tempSp, st);

            var mi = st.GetMethod("ConfigureModuleServices", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (mi == null)
            {
                Console.WriteLine($"[Bootstrap] No ConfigureModuleServices method found on {st.FullName}");
                continue;
            }

            try
            {
                mi.Invoke(instance, new object[] { services });
                Console.WriteLine($"[Bootstrap] Invoked ConfigureModuleServices on {st.FullName}");
            }
            catch (TargetInvocationException tie)
            {
                Console.WriteLine($"[Bootstrap] ConfigureModuleServices threw in {st.FullName}: {tie.InnerException?.Message}");
                Console.WriteLine(tie.InnerException?.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Bootstrap] Invoke failed for {st.FullName}: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Bootstrap] Failed to create instance of {st.FullName}: {ex.Message}");
        }
    }
}



// ============================================================================
// RUN HOST MIGRATIONS AFTER MODULE STARTUP COMPLETED
// ============================================================================
async Task RunHostMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;

    var logger = sp.GetRequiredService<ILoggerFactory>()
        .CreateLogger("HostMigrations");

    var opts = sp.GetRequiredService<IOptions<ModuleRegistryOptions>>();
    var runner = sp.GetRequiredService<IMigrationRunnerService>();
    var cfg = sp.GetRequiredService<IConfiguration>();

    var hostConn = cfg.GetConnectionString("Host");
    if (string.IsNullOrWhiteSpace(hostConn))
        throw new InvalidOperationException("ConnectionStrings:Host is not configured.");

    if (opts.Value.MigrationAssemblies.Count == 0)
    {
        logger.LogWarning("No migration assemblies registered. Did module ConfigureModuleServices run?");
        return;
    }

    logger.LogInformation("Running host migrations for {Count} assemblies", opts.Value.MigrationAssemblies.Count);

    foreach (var asm in opts.Value.MigrationAssemblies)
    {
        logger.LogInformation("Migrating assembly: {Asm}", asm.FullName);
        await runner.RunMigrationsAsync(hostConn, asm);
    }

    logger.LogInformation("Host migrations completed.");
}
