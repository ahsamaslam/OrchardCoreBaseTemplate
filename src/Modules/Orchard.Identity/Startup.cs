// src/Modules/Orchard.Identity/Startup.cs
using LinqToDB.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ModuleBase;
using System;

namespace Orchard.Identity
{
    public class Startup : ModuleStartupBase
    {
        public override string ModuleName => "Orchard.Identity";

        protected override void ConfigureModuleServices(IServiceCollection services)
        {
            // Register this module's migration assembly so Host can run migrations.
            // Pass the migration *type*, not the Assembly object.
            RegisterMigrationAssembly(services, typeof(Migrations._0001_CreateIdentityTables));

            // Register the Identity user store (tenant-scoped factory will be provided by Host)
            // Ensure Services.IdentityUserStore is public and implements IUserStore<Models.IdentityUser>
            services.AddScoped<IUserStore<Models.IdentityUser>, Services.IdentityUserStore>();

            // Register other identity related services
            services.AddScoped<Services.IUserService, Services.UserService>();

            // NOTE: TenantLinq2DbFactory should be registered in Host (recommended).
            // If you must register a factory here for local testing, make sure the class is available
            // and you understand it couples Module -> Host assembly. Prefer Host registration:
            // builder.Services.AddSingleton<ITenantScopedFactory<DataConnection>, TenantLinq2DbFactory>();
        }

        protected override void ConfigureModule(Microsoft.AspNetCore.Builder.IApplicationBuilder app,
                                                Microsoft.AspNetCore.Routing.IEndpointRouteBuilder routes,
                                                IServiceProvider sp)
        {
            // If you need to run code at application startup (e.g., seed admin user),
            // resolve IMigrationRunnerService / ISettingsStore / etc. from sp and act.
        }
    }
}
