OrchardApp Skeleton

Structure:
- src/OrchardApp.Host : the startup web host project. References OrchardCore.Application.Cms.Targets
- src/Modules/* : sample Orchard Core modules (Setup, TenantManagement, Identity, ExternalAuth.AzureAD, RedisCache, Settings, Resources, Lucene)
- src/Themes/* : sample themes (Admin.Default, Admin.Alt, Frontend.Default)

Architecture:
- Host project configures Orchard Core and the middleware pipeline (security headers, auth, localization).
- Modules are compiled class libraries using OrchardCore.Module.Targets and will be discovered by Orchard Core.
- Themes are using OrchardCore.Theme.Targets and will be available as selectable themes.
- Tenants are created at runtime via the Orchard setup UI. You can create TenantA, TenantB, TenantC each with their own DB.

Next steps after opening in Visual Studio:
1. Restore NuGet packages (ensure your NuGet sources are configured; OrchardCore packages are on nuget.org).
2. Build solution.
3. Run OrchardApp.Host.
