using Hubion.Domain.Entities;

namespace Hubion.Application.Interfaces.Repositories;

public interface IFlowSessionRepository
{
    Task<FlowSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<FlowSession?> GetActiveByCallRecordAsync(Guid callRecordId, CancellationToken ct = default);
    Task AddAsync(FlowSession session, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
