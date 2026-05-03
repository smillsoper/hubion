using ContactConnection.Application.Interfaces.Repositories;
using ContactConnection.Domain.Entities;
using ContactConnection.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactConnection.Infrastructure.Repositories;

public class FlowSessionRepository : IFlowSessionRepository
{
    private readonly ScopedTenantDbContextFactory _factory;
    private TenantDbContext? _db;
    private TenantDbContext Db => _db ??= _factory.Create();

    public FlowSessionRepository(ScopedTenantDbContextFactory factory) => _factory = factory;

    public Task<FlowSession?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Db.FlowSessions.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<FlowSession?> GetActiveByCallRecordAsync(Guid callRecordId, CancellationToken ct = default) =>
        Db.FlowSessions.FirstOrDefaultAsync(
            s => s.CallRecordId == callRecordId && s.Status == FlowSessionStatus.Active, ct);

    public async Task AddAsync(FlowSession session, CancellationToken ct = default) =>
        await Db.FlowSessions.AddAsync(session, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        Db.SaveChangesAsync(ct);
}
