using Hubion.Application.Interfaces.Repositories;
using Hubion.Domain.Entities;
using Hubion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hubion.Infrastructure.Repositories;

public class CallRecordRepository : ICallRecordRepository
{
    private readonly TenantDbContext _db;

    public CallRecordRepository(TenantDbContext db) => _db = db;

    public Task<CallRecord?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.CallRecords.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<CallRecord?> GetByIdWithInteractionsAsync(Guid id, CancellationToken ct = default) =>
        _db.CallRecords
            .Include(r => r.Interactions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(CallRecord record, CancellationToken ct = default) =>
        await _db.CallRecords.AddAsync(record, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
