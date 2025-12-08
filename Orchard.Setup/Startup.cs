using Orchard.ModuleBase;
using OrchardCore.Recipes.Services;

namespace Orchard.Setup
{
    public class Startup : ModuleStartupBase
    {
        protected override void ConfigureModuleServices(IServiceCollection services)
        {
            // Recipe executor provided by Orchard Recipes package
            services.AddScoped<IRecipeExecutor, RecipeExecutor>();

            // A guard service to know if setup is needed or already done:
            services.AddScoped<ISetupStateService, SetupStateService>();

            // Make SetupController available
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation(); // optional for dev experience
        }

        protected override void ConfigureModule(
            IApplicationBuilder app,
            IEndpointRouteBuilder routes,
            IServiceProvider sp)
        {
            // Map admin/setup route under Host root: "/Setup"
            routes.MapAreaControllerRoute(
                name: "Setup",
                areaName: "Orchard.Setup",
                pattern: "Setup/{action=Index}/{id?}",
                defaults: new { controller = "Setup" });
        }
    }
}
