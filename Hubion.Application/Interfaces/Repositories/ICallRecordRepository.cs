using Hubion.Domain.Entities;

namespace Hubion.Application.Interfaces.Repositories;

public interface ICallRecordRepository
{
    Task<CallRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CallRecord?> GetByIdWithInteractionsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(CallRecord record, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
