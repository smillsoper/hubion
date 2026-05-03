namespace ContactConnection.Domain.Entities;

/// <summary>
/// An active recurring shipment agreement created when an order line with AutoShip = true
/// is committed. Each subscription represents one recurring product.
///
/// The Worker service (SubscriptionProcessingService) queries IsDue() daily and creates
/// a new Order + OrderLine for each due subscription, then calls RecordShipment() to
/// advance the schedule.
///
/// Prices are locked at the time the subscription is created (snapshot from the original
/// order line) — changes to the Offer after enrollment do not affect existing subscriptions.
/// </summary>
public class Subscription
{
    // Identity
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }

    // Source references
    public Guid? CallRecordId { get; private set; }    // the call that created this subscription
    public Guid OriginalOrderId { get; private set; }  // first order that enrolled this subscription
    public Guid OriginalOrderLineId { get; private set; }

    // Product/offer snapshot — frozen at enrollment time
    public Guid OfferId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = "";
    public string Description { get; private set; } = "";
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Shipping { get; private set; }

    // Schedule
    public int IntervalDays { get; private set; }
    public DateTimeOffset NextShipDate { get; private set; }
    public DateTimeOffset? LastShipDate { get; private set; }
    public int ShipmentCount { get; private set; }

    // Lifecycle
    public string Status { get; private set; } = SubscriptionStatus.Active;

    // Audit
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    // Required by EF Core
    private Subscription() { }

    /// <summary>
    /// Creates a subscription from an AutoShip order line.
    /// The first shipment was already fulfilled as part of the original order,
    /// so NextShipDate starts at <c>now + IntervalDays</c>.
    /// </summary>
    public static Subscription CreateFromOrderLine(
        Guid tenantId,
        Guid? callRecordId,
        Guid originalOrderId,
        OrderLine line)
    {
        var now = DateTimeOffset.UtcNow;
        return new Subscription
        {
            Id                  = Guid.NewGuid(),
            TenantId            = tenantId,
            CallRecordId        = callRecordId,
            OriginalOrderId     = originalOrderId,
            OriginalOrderLineId = line.Id,
            OfferId             = line.OfferId,
            ProductId           = line.ProductId,
            Sku                 = line.Sku,
            Description         = line.Description,
            Quantity            = line.Quantity,
            UnitPrice           = line.UnitPrice,
            Shipping            = line.Shipping,
            IntervalDays        = line.AutoShipIntervalDays > 0 ? line.AutoShipIntervalDays : 30,
            NextShipDate        = now.AddDays(line.AutoShipIntervalDays > 0 ? line.AutoShipIntervalDays : 30),
            ShipmentCount       = 0,
            Status              = SubscriptionStatus.Active,
            CreatedAt           = now,
            UpdatedAt           = now
        };
    }

    /// <summary>Whether this subscription is due to ship now.</summary>
    public bool IsDue() =>
        Status == SubscriptionStatus.Active && NextShipDate <= DateTimeOffset.UtcNow;

    /// <summary>Records a successful shipment and advances the schedule.</summary>
    public void RecordShipment()
    {
        var now = DateTimeOffset.UtcNow;
        ShipmentCount++;
        LastShipDate  = now;
        NextShipDate  = now.AddDays(IntervalDays);
        UpdatedAt     = now;
    }

    public void Pause()
    {
        Status    = SubscriptionStatus.Paused;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Resume()
    {
        Status    = SubscriptionStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        Status      = SubscriptionStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
        UpdatedAt   = DateTimeOffset.UtcNow;
    }
}

/// <summary>Subscription lifecycle statuses.</summary>
public static class SubscriptionStatus
{
    public const string Active    = "active";
    public const string Paused    = "paused";
    public const string Cancelled = "cancelled";
}
