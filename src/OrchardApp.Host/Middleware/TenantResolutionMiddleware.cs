public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;

        // Resolve scoped ITenantStore from the per-request service provider
        var tenantStore = context.RequestServices.GetRequiredService<ITenantStore>();

        var tenant = await tenantStore.FindByHostAsync(host);

        if (tenant != null)
        {
            context.Items["CurrentTenant"] = tenant;
            _logger.LogInformation("Resolved tenant {Tenant}", tenant.TenantId);
        }
        else
        {
            _logger.LogWarning("Tenant not found for host {Host}", host);
        }

        await _next(context);
    }
}
