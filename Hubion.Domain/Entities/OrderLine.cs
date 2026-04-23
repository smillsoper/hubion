using Hubion.Domain.ValueObjects.Commerce;

namespace Hubion.Domain.Entities;

/// <summary>
/// A single line item in an order — a point-in-time snapshot of a CartItem at commit time.
/// Prices, quantities, and offer details are frozen here; changing a Product or Offer later
/// does not affect existing order lines.
///
/// Tracks fulfillment state (status, tracking number, timestamps) independently
/// from other lines in the same order to support partial shipments.
/// </summary>
public class OrderLine
{
    // Identity
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid TenantId { get; private set; }

    // Offer/product snapshot — frozen at commit time
    public Guid OfferId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = "";
    public string Description { get; private set; } = "";

    // Quantity and pricing snapshot
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal ExtendedPrice { get; private set; }
    public decimal Shipping { get; private set; }
    public decimal SalesTax { get; private set; }
    public decimal Weight { get; private set; }

    // Tax / shipping flags
    public bool ShippingExempt { get; private set; }
    public bool TaxExempt { get; private set; }
    public bool OnBackOrder { get; private set; }

    // AutoShip
    public bool AutoShip { get; private set; }
    public int AutoShipIntervalDays { get; private set; }

    // Sales metadata
    public bool IsUpsell { get; private set; }
    public string? MixMatchCode { get; private set; }

    // Shipping detail
    public string? ShipMethod { get; private set; }
    public string? DeliveryMessage { get; private set; }
    public string? ShipToJson { get; private set; }   // serialized AddressData

    // Geographic surcharges
    public decimal CanadaSurcharge { get; private set; }
    public decimal AKHISurcharge { get; private set; }
    public decimal OutlyingUSSurcharge { get; private set; }
    public decimal ForeignSurcharge { get; private set; }

    // JSONB — complex structures snapshotted from cart item
    public List<PaymentInstallment> Payments { get; private set; } = [];
    public List<CartPersonalizationAnswer> PersonalizationAnswers { get; private set; } = [];
    public List<CartKitSelection> KitSelections { get; private set; } = [];

    // Fulfillment
    public string FulfillmentStatus { get; private set; } = OrderLineStatus.Pending;
    public string? TrackingNumber { get; private set; }
    public DateTimeOffset? ShippedAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    // Audit
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation
    public Order Order { get; private set; } = null!;

    // Required by EF Core
    private OrderLine() { }

    /// <summary>
    /// Creates an order line by snapshotting a CartItem at order commit time.
    /// </summary>
    public static OrderLine FromCartItem(Guid orderId, Guid tenantId, CartItem item)
    {
        var now = DateTimeOffset.UtcNow;
        return new OrderLine
        {
            Id                    = Guid.NewGuid(),
            OrderId               = orderId,
            TenantId              = tenantId,
            OfferId               = item.OfferId,
            ProductId             = item.ProductId,
            Sku                   = item.Sku,
            Description           = item.Description,
            Quantity              = item.Quantity,
            UnitPrice             = item.FullPrice,
            ExtendedPrice         = item.ExtendedPrice,
            Shipping              = item.Shipping,
            SalesTax              = item.SalesTax,
            Weight                = item.Weight,
            ShippingExempt        = item.ShippingExempt,
            TaxExempt             = item.TaxExempt,
            OnBackOrder           = item.OnBackOrder,
            AutoShip              = item.AutoShip,
            AutoShipIntervalDays  = item.AutoShipIntervalDays,
            IsUpsell              = item.IsUpsell,
            MixMatchCode          = item.MixMatchCode,
            ShipMethod            = item.ShipMethod,
            DeliveryMessage       = item.DeliveryMessage,
            ShipToJson            = item.ShipToJson,
            CanadaSurcharge       = item.CanadaSurcharge,
            AKHISurcharge         = item.AKHISurcharge,
            OutlyingUSSurcharge   = item.OutlyingUSSurcharge,
            ForeignSurcharge      = item.ForeignSurcharge,
            Payments              = item.Payments,
            PersonalizationAnswers = item.PersonalizationAnswers,
            KitSelections         = item.KitSelections,
            FulfillmentStatus     = OrderLineStatus.Pending,
            CreatedAt             = now,
            UpdatedAt             = now
        };
    }

    /// <summary>
    /// Creates an order line from a subscription renewal (no cart involved).
    /// Prices are the subscription's locked-in snapshot values.
    /// </summary>
    public static OrderLine CreateFromSubscription(Guid orderId, Guid tenantId, Subscription sub)
    {
        var now = DateTimeOffset.UtcNow;
        return new OrderLine
        {
            Id                    = Guid.NewGuid(),
            OrderId               = orderId,
            TenantId              = tenantId,
            OfferId               = sub.OfferId,
            ProductId             = sub.ProductId,
            Sku                   = sub.Sku,
            Description           = sub.Description,
            Quantity              = sub.Quantity,
            UnitPrice             = sub.UnitPrice,
            ExtendedPrice         = sub.UnitPrice * sub.Quantity,
            Shipping              = sub.Shipping,
            SalesTax              = 0,
            Weight                = 0,
            AutoShip              = true,
            AutoShipIntervalDays  = sub.IntervalDays,
            FulfillmentStatus     = OrderLineStatus.Pending,
            CreatedAt             = now,
            UpdatedAt             = now
        };
    }

    /// <summary>Marks this line as shipped with a tracking number.</summary>
    public void Ship(string trackingNumber)
    {
        FulfillmentStatus = OrderLineStatus.Shipped;
        TrackingNumber    = trackingNumber;
        ShippedAt         = DateTimeOffset.UtcNow;
        UpdatedAt         = DateTimeOffset.UtcNow;
    }

    /// <summary>Marks this line as delivered.</summary>
    public void MarkDelivered()
    {
        FulfillmentStatus = OrderLineStatus.Delivered;
        DeliveredAt       = DateTimeOffset.UtcNow;
        UpdatedAt         = DateTimeOffset.UtcNow;
    }

    /// <summary>Cancels this line.</summary>
    public void Cancel()
    {
        FulfillmentStatus = OrderLineStatus.Cancelled;
        CancelledAt       = DateTimeOffset.UtcNow;
        UpdatedAt         = DateTimeOffset.UtcNow;
    }
}

/// <summary>Order line fulfillment statuses.</summary>
public static class OrderLineStatus
{
    public const string Pending    = "pending";
    public const string Processing = "processing";
    public const string Shipped    = "shipped";
    public const string Delivered  = "delivered";
    public const string Cancelled  = "cancelled";
    public const string Returned   = "returned";
}
