using ContactConnection.Domain.Entities;

namespace ContactConnection.Application.Interfaces.Repositories;

public interface IFlowRepository
{
    Task<Flow?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Flow>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<List<Flow>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Flow flow, CancellationToken ct = default);
    void Delete(Flow flow);
    Task SaveChangesAsync(CancellationToken ct = default);
}
