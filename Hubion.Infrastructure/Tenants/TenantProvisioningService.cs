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
    private readonly ITenantDbContextFactory _tenantDbContextFactory;

    public TenantProvisioningService(
        ITenantRepository tenants,
        HubionDbContext db,
        ITenantDbContextFactory tenantDbContextFactory)
    {
        _tenants = tenants;
        _db = db;
        _tenantDbContextFactory = tenantDbContextFactory;
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

        // 1. Create the PostgreSQL schema
        // SchemaName is system-generated ("tenant_" + normalized subdomain) — not user input.
        var schemaDdl = $"CREATE SCHEMA IF NOT EXISTS \"{tenant.SchemaName}\"";
#pragma warning disable EF1002
        await _db.Database.ExecuteSqlRawAsync(schemaDdl, ct);
#pragma warning restore EF1002

        // 2. Apply all tenant-scoped EF migrations to the new schema
        await using var tenantCtx = _tenantDbContextFactory.Create(tenant.SchemaName);
        await tenantCtx.Database.MigrateAsync(ct);

        // 3. Save the tenant record — all three steps succeed or the transaction rolls back
        await _tenants.SaveChangesAsync(ct);

        return tenant;
    }
}
