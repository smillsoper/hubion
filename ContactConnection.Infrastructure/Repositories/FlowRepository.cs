using ContactConnection.Application.Interfaces.Repositories;
using ContactConnection.Domain.Entities;
using ContactConnection.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactConnection.Infrastructure.Repositories;

public class FlowRepository : IFlowRepository
{
    private readonly ScopedTenantDbContextFactory _factory;
    private TenantDbContext? _db;
    private TenantDbContext Db => _db ??= _factory.Create();

    public FlowRepository(ScopedTenantDbContextFactory factory) => _factory = factory;

    public Task<Flow?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Db.Flows.FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<List<Flow>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        Db.Flows.Where(f => f.TenantId == tenantId && f.IsActive)
                .OrderBy(f => f.Name)
                .ToListAsync(ct);

    public async Task AddAsync(Flow flow, CancellationToken ct = default) =>
        await Db.Flows.AddAsync(flow, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        Db.SaveChangesAsync(ct);
}
