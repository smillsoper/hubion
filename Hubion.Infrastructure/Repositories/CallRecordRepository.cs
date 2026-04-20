using Hubion.Application.Interfaces.Repositories;
using Hubion.Domain.Entities;
using Hubion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hubion.Infrastructure.Repositories;

public class CallRecordRepository : ICallRecordRepository
{
    private readonly ScopedTenantDbContextFactory _factory;
    private TenantDbContext? _db;
    private TenantDbContext Db => _db ??= _factory.Create();

    public CallRecordRepository(ScopedTenantDbContextFactory factory) => _factory = factory;

    public Task<CallRecord?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Db.CallRecords.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<CallRecord?> GetByIdWithInteractionsAsync(Guid id, CancellationToken ct = default) =>
        Db.CallRecords
            .Include(r => r.Interactions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(CallRecord record, CancellationToken ct = default) =>
        await Db.CallRecords.AddAsync(record, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        Db.SaveChangesAsync(ct);
}
