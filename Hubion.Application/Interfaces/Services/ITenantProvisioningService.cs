using Hubion.Domain.Entities;

namespace Hubion.Application.Interfaces.Services;

public interface ITenantProvisioningService
{
    /// <summary>
    /// Creates the tenant record and provisions its dedicated PostgreSQL schema.
    /// </summary>
    Task<Tenant> ProvisionAsync(
        string name,
        string subdomain,
        string planTier,
        string timezone,
        CancellationToken ct = default);
}
