using ContactConnection.Domain.Entities;

namespace ContactConnection.Application.Interfaces.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Subscription>> GetByCallRecordIdAsync(Guid callRecordId, CancellationToken ct = default);

    /// <summary>
    /// Returns all active subscriptions whose NextShipDate is on or before <paramref name="cutoff"/>.
    /// Used by the Worker service to find subscriptions due to ship.
    /// </summary>
    Task<List<Subscription>> GetDueAsync(DateTimeOffset cutoff, CancellationToken ct = default);

    Task AddAsync(Subscription subscription, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Subscription> subscriptions, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
