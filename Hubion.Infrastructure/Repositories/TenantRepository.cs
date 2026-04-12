using Hubion.Application.Interfaces.Repositories;
using Hubion.Domain.Entities;
using Hubion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hubion.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly HubionDbContext _db;

    public TenantRepository(HubionDbContext db) => _db = db;

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default) =>
        _db.Tenants.FirstOrDefaultAsync(t => t.Subdomain == subdomain.ToLowerInvariant(), ct);

    public Task<Tenant?> GetByCustomDomainAsync(string customDomain, CancellationToken ct = default) =>
        _db.Tenants.FirstOrDefaultAsync(t => t.CustomDomain == customDomain.ToLowerInvariant(), ct);

    public Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken ct = default) =>
        _db.Tenants.AnyAsync(t => t.Subdomain == subdomain.ToLowerInvariant(), ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default) =>
        await _db.Tenants.AddAsync(tenant, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
