using ContactConnection.Domain.ValueObjects.Commerce;

namespace ContactConnection.Application.Interfaces.Services;

/// <summary>
/// Manages soft inventory reservations for cart operations.
///
/// Lifecycle:
///   Reserve  — called when a cart is set; holds units against QtyAvailable so they
///              cannot be sold to another caller while this cart is active.
///   Release  — called when a cart is replaced or cleared; frees the held units.
///   Confirm  — called on order commit; converts the soft hold into a real decrement
///              of QtyAvailable and clears the reservation.
///
/// Only products with DecrementOnOrder = true participate in reservation tracking.
/// Discontinued and NoBackorder products with insufficient free stock are rejected at Reserve time.
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Attempts to reserve inventory for all items in the cart.
    /// Returns a list of SKUs that could not be reserved (empty = full success).
    /// On partial failure, no reservations are applied (all-or-nothing).
    /// </summary>
    Task<List<string>> ReserveCartAsync(CartDocument cart, CancellationToken ct = default);

    /// <summary>
    /// Releases previously reserved inventory for all items in the cart.
    /// Safe to call on an empty or null cart.
    /// </summary>
    Task ReleaseCartAsync(CartDocument? cart, CancellationToken ct = default);

    /// <summary>
    /// Confirms reserved inventory on order commit — decrements QtyAvailable
    /// and clears QtyReserved for all items in the cart.
    /// </summary>
    Task ConfirmCartAsync(CartDocument cart, CancellationToken ct = default);
}
