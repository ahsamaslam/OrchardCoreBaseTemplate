using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using System.Reflection;

namespace Orchard.ModuleBase;

public abstract class ModuleStartupBase : StartupBase
{
    public virtual string ModuleName => GetType().Assembly.GetName().Name!;

    public sealed override void ConfigureServices(IServiceCollection services)
    {
        ConfigureModuleServices(services);
    }

    public sealed override void Configure(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider)
    {
        ConfigureModule(app, routes, serviceProvider);
    }

    protected virtual void ConfigureModuleServices(IServiceCollection services) { }

    protected virtual void ConfigureModule(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider sp)
    { }

    protected void RegisterMigrationAssembly(IServiceCollection services, Type migrationType)
    {
        services.Configure<ModuleRegistryOptions>(o =>
        {
            o.MigrationAssemblies.Add(migrationType.Assembly);
        });
    }
}

public class ModuleRegistryOptions
{
    public HashSet<Assembly> MigrationAssemblies { get; set; } = new();
}
