using ContactConnection.Domain.Entities;

namespace ContactConnection.Application.Interfaces.Services;

/// <summary>
/// Creates a renewal Order + OrderLine from a due subscription.
/// Used by SubscriptionProcessingService in ContactConnection.Worker.
///
/// Confirms inventory (if product tracks inventory) before creating the order.
/// Throws InvalidOperationException if insufficient stock for a NoBackorder product.
/// </summary>
public interface ISubscriptionOrderCreator
{
    Task<Order> CreateRenewalOrderAsync(Subscription subscription, CancellationToken ct = default);
}
