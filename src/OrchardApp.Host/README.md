OrchardApp.Host - Minimal Orchard Core Host (skeleton)

How to run:
1. Open the solution OrchardApp.sln in Visual Studio (2022/2023 recommended) or use the `dotnet` CLI.
2. Restore packages: `dotnet restore src/OrchardApp.Host/OrchardApp.Host.csproj`
3. Set OrchardApp.Host as the Startup project and run (F5) or `dotnet run --project src/OrchardApp.Host/OrchardApp.Host.csproj`
4. On first run you'll be redirected to the Orchard setup where you can create tenants, admin user, and run recipes.

Notes:
- This skeleton references OrchardCore.Application.Cms.Targets v2.2.1. You may choose a different/stable version.
- Add any external services (Redis, SQL Server, Azure AD) by configuring services in Program.cs and appsettings.json.
