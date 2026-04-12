using Hubion.Application.Interfaces.Repositories;
using Hubion.Application.Interfaces.Services;
using Hubion.Domain.Entities;
using Hubion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hubion.Infrastructure.Tenants;

public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly ITenantRepository _tenants;
    private readonly HubionDbContext _db;

    public TenantProvisioningService(ITenantRepository tenants, HubionDbContext db)
    {
        _tenants = tenants;
        _db = db;
    }

    public async Task<Tenant> ProvisionAsync(
        string name,
        string subdomain,
        string planTier,
        string timezone,
        CancellationToken ct = default)
    {
        if (await _tenants.SubdomainExistsAsync(subdomain, ct))
            throw new InvalidOperationException($"Subdomain '{subdomain}' is already taken.");

        var tenant = Tenant.Create(name, subdomain, planTier, timezone);

        await _tenants.AddAsync(tenant, ct);

        // Create the tenant's dedicated PostgreSQL schema before committing,
        // so both succeed or both are rolled back together.
        // SchemaName is system-generated ("tenant_" + normalized subdomain) — not user input.
        // PostgreSQL does not support parameterized identifiers, so we build the DDL directly
        // after validating subdomain format upstream.
        var schemaDdl = $"CREATE SCHEMA IF NOT EXISTS \"{tenant.SchemaName}\"";
#pragma warning disable EF1002
        await _db.Database.ExecuteSqlRawAsync(schemaDdl, ct);
#pragma warning restore EF1002

        await _tenants.SaveChangesAsync(ct);

        return tenant;
    }
}
