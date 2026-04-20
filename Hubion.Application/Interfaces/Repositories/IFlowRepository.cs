using Hubion.Domain.Entities;

namespace Hubion.Application.Interfaces.Repositories;

public interface IFlowRepository
{
    Task<Flow?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Flow>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Flow flow, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
