using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Setup
{
    public class Startup : IModularTenantEvents
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // register services here
        }
    }
}
