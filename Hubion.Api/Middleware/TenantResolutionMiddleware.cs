using Hubion.Application.Interfaces.Repositories;
using Hubion.Domain.Entities;

namespace Hubion.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantRepository tenants)
    {
        // Nginx forwards the subdomain in X-Tenant-Subdomain.
        // Fall back to parsing the host header directly for local dev without nginx.
        var subdomain = context.Request.Headers["X-Tenant-Subdomain"].FirstOrDefault()
            ?? ExtractSubdomain(context.Request.Host.Host);

        if (!string.IsNullOrEmpty(subdomain))
        {
            var tenant = await tenants.GetBySubdomainAsync(subdomain);
            if (tenant is not null)
                context.Items["Tenant"] = tenant;
        }

        await _next(context);
    }

    private static string? ExtractSubdomain(string host)
    {
        // tms.hubion.local → tms
        var parts = host.Split('.');
        return parts.Length >= 3 ? parts[0] : null;
    }
}

public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app) =>
        app.UseMiddleware<TenantResolutionMiddleware>();
}
