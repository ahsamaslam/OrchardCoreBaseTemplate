using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ModuleBase;
using Orchard.TenantManagement.Services;
using System;
namespace Orchard.TenantManagement
{
    public class Startup : ModuleStartupBase
    {
        protected override void ConfigureModuleServices(IServiceCollection services)
        {
            RegisterMigrationAssembly(services, typeof(Migrations._0001_CreateTenantsTable));
            // Tenant manager service that wraps Orchard shell APIs
        }

        protected override void ConfigureModule(
            IApplicationBuilder app,
            IEndpointRouteBuilder routes,
            IServiceProvider sp)
        {
        

            //// Admin area route for tenant management (under /Admin/Tenants)
            //routes.MapAreaControllerRoute(
            //    name: "TenantsAdmin",
            //    areaName: "Orchard.TenantManagement",
            //    pattern: "Admin/Tenants/{action=Index}/{id?}",
            //    defaults: new { controller = "Tenants", action = "Index" });
        }
    }
}
