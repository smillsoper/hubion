using Hubion.Domain.Entities;

namespace Hubion.Application.Interfaces.Repositories;

public interface IOfferRepository
{
    Task<Offer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Offer>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Returns all active offers for the current tenant, ordered by name.</summary>
    Task<List<Offer>> GetActiveAsync(CancellationToken ct = default);

    Task AddAsync(Offer offer, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
