using Hubion.Domain.Entities;
using Hubion.Domain.ValueObjects.Commerce;
using Xunit;

namespace Hubion.Domain.Tests.Domain;

public class OrderLineLifecycleTests
{
    private static CartItem MakeCartItem(
        bool autoShip = false,
        int autoShipIntervalDays = 0,
        List<PaymentInstallment>? payments = null) => new(
        OfferId: Guid.NewGuid(),
        ProductId: Guid.NewGuid(),
        Sku: "SKU001",
        Description: "Test Product",
        Quantity: 1,
        FullPrice: 29.95m,
        ExtendedPrice: 29.95m,
        Shipping: 5.95m,
        Weight: 1.0m,
        SalesTax: 0m,
        ShippingExempt: false,
        TaxExempt: false,
        OnBackOrder: false,
        AutoShip: autoShip,
        AutoShipIntervalDays: autoShipIntervalDays,
        IsUpsell: false,
        UpsellQty: 0,
        MixMatchCode: null,
        ShipMethod: null,
        DeliveryMessage: null,
        ShipToJson: null,
        Payments: payments ?? [],
        PersonalizationAnswers: [],
        KitSelections: [],
        CanadaSurcharge: 0m,
        AKHISurcharge: 0m,
        OutlyingUSSurcharge: 0m,
        ForeignSurcharge: 0m);

    internal static OrderLine MakeLine(Guid? orderId = null, Guid? tenantId = null, bool autoShip = false, int intervalDays = 30)
    {
        var oid = orderId ?? Guid.NewGuid();
        var tid = tenantId ?? Guid.NewGuid();
        return OrderLine.FromCartItem(oid, tid, MakeCartItem(autoShip, intervalDays));
    }

    [Fact]
    public void FromCartItem_SnapshotsFields()
    {
        var item = MakeCartItem();
        var line = OrderLine.FromCartItem(Guid.NewGuid(), Guid.NewGuid(), item);

        Assert.Equal("SKU001", line.Sku);
        Assert.Equal(29.95m, line.UnitPrice);
        Assert.Equal(OrderLineStatus.Pending, line.FulfillmentStatus);
        Assert.Null(line.TrackingNumber);
        Assert.Null(line.ShippedAt);
        Assert.Null(line.DeliveredAt);
        Assert.Null(line.CancelledAt);
    }

    [Fact]
    public void Ship_SetsShippedStatusAndTracking()
    {
        var before = DateTimeOffset.UtcNow;
        var line = MakeLine();
        line.Ship("TRACK123");

        Assert.Equal(OrderLineStatus.Shipped, line.FulfillmentStatus);
        Assert.Equal("TRACK123", line.TrackingNumber);
        Assert.NotNull(line.ShippedAt);
        Assert.True(line.ShippedAt >= before);
    }

    [Fact]
    public void MarkDelivered_SetsDeliveredStatus()
    {
        var before = DateTimeOffset.UtcNow;
        var line = MakeLine();
        line.Ship("TRACK123");
        line.MarkDelivered();

        Assert.Equal(OrderLineStatus.Delivered, line.FulfillmentStatus);
        Assert.NotNull(line.DeliveredAt);
        Assert.True(line.DeliveredAt >= before);
    }

    [Fact]
    public void Cancel_SetsCancelledStatus()
    {
        var before = DateTimeOffset.UtcNow;
        var line = MakeLine();
        line.Cancel();

        Assert.Equal(OrderLineStatus.Cancelled, line.FulfillmentStatus);
        Assert.NotNull(line.CancelledAt);
        Assert.True(line.CancelledAt >= before);
    }

    [Fact]
    public void CreateFromSubscription_SnapshotsSubscriptionFields()
    {
        var sub = SubscriptionLifecycleTests.MakeSubscription();
        var line = OrderLine.CreateFromSubscription(Guid.NewGuid(), sub.TenantId, sub);

        Assert.Equal(sub.Sku, line.Sku);
        Assert.Equal(sub.Quantity, line.Quantity);
        Assert.Equal(sub.UnitPrice, line.UnitPrice);
        Assert.Equal(sub.UnitPrice * sub.Quantity, line.ExtendedPrice);
        Assert.True(line.AutoShip);
        Assert.Equal(sub.IntervalDays, line.AutoShipIntervalDays);
        Assert.Equal(OrderLineStatus.Pending, line.FulfillmentStatus);
    }
}
