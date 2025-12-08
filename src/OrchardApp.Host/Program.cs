
var builder = WebApplication.CreateBuilder(args);

// Logging, config, secrets (KeyVault) go here
builder.Host.ConfigureLogging(logging => {
    // configure Serilog/ILogger
});

// Add OrchardCore as application framework
builder.Services.AddOrchardCore()
    .AddMvc()
    .AddShellFeatures() // optional: default features
    .ConfigureServices((ctx, services) => {
        // Register host-level services, migration runner, etc.
        services.AddSingleton<IMigrationRunnerService, MigrationRunnerService>();
        services.AddSingleton<ITenantMigrationRunner, TenantMigrationRunner>();
    });

var app = builder.Build();

// RequestLocalization and host-level middleware
app.UseRequestLocalization(); // configure cultures in ConfigureServices
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Let Orchard handle modules & routing
app.UseOrchardCore();

app.Run();
