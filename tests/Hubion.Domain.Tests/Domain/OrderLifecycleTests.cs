using Hubion.Domain.Entities;
using Hubion.Domain.ValueObjects.Commerce;
using Xunit;

namespace Hubion.Domain.Tests.Domain;

public class OrderLifecycleTests
{
    private static Order MakeOrder(List<OrderLine>? lines = null)
    {
        var id       = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var lineList = lines ?? [OrderLineLifecycleTests.MakeLine(id, tenantId)];
        return Order.CreateFromCart(id, tenantId, null, CartDocument.Empty(), lineList);
    }

    private static List<OrderLine> MakeLines(Guid orderId, Guid tenantId, int count)
    {
        var lines = new List<OrderLine>(count);
        for (var i = 0; i < count; i++)
            lines.Add(OrderLineLifecycleTests.MakeLine(orderId, tenantId));
        return lines;
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_SetsStatusAndCancelledAt()
    {
        var before = DateTimeOffset.UtcNow;
        var order  = MakeOrder();
        order.Cancel();
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.NotNull(order.CancelledAt);
        Assert.True(order.CancelledAt >= before);
    }

    // ── RefreshStatus ─────────────────────────────────────────────────────────

    [Fact]
    public void RefreshStatus_AllPending_StatusRemainsConfirmed()
    {
        var order = MakeOrder();
        // All lines start as Pending
        order.RefreshStatus();
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void RefreshStatus_AllLinesCancelled_OrderBecomeCancelled()
    {
        var id       = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var lines    = MakeLines(id, tenantId, 2);
        var order    = Order.CreateFromCart(id, tenantId, null, CartDocument.Empty(), lines);
        lines[0].Cancel();
        lines[1].Cancel();
        order.RefreshStatus();
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void RefreshStatus_SomeLinesShipped_OrderBecomesPartiallyShipped()
    {
        var id       = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var lines    = MakeLines(id, tenantId, 2);
        var order    = Order.CreateFromCart(id, tenantId, null, CartDocument.Empty(), lines);
        lines[0].Ship("TRACK001"); // one shipped
        // lines[1] stays Pending
        order.RefreshStatus();
        Assert.Equal(OrderStatus.PartiallyShipped, order.Status);
        Assert.NotNull(order.ShippedAt);
    }

    [Fact]
    public void RefreshStatus_AllLinesShipped_OrderBecomesShipped()
    {
        var id       = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var lines    = MakeLines(id, tenantId, 2);
        var order    = Order.CreateFromCart(id, tenantId, null, CartDocument.Empty(), lines);
        lines[0].Ship("TRACK001");
        lines[1].Ship("TRACK002");
        order.RefreshStatus();
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.NotNull(order.ShippedAt);
    }

    [Fact]
    public void RefreshStatus_AllLinesDelivered_OrderBecomesDelivered()
    {
        var id       = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var lines    = MakeLines(id, tenantId, 2);
        var order    = Order.CreateFromCart(id, tenantId, null, CartDocument.Empty(), lines);
        lines[0].Ship("T1"); lines[0].MarkDelivered();
        lines[1].Ship("T2"); lines[1].MarkDelivered();
        order.RefreshStatus();
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.NotNull(order.DeliveredAt);
    }

    [Fact]
    public void RefreshStatus_DeliveredAndCancelled_OrderBecomesDelivered()
    {
        var id       = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var lines    = MakeLines(id, tenantId, 2);
        var order    = Order.CreateFromCart(id, tenantId, null, CartDocument.Empty(), lines);
        lines[0].Ship("T1"); lines[0].MarkDelivered();
        lines[1].Cancel();
        order.RefreshStatus();
        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void RefreshStatus_ShippedAndCancelled_OrderBecomesShipped()
    {
        var id       = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var lines    = MakeLines(id, tenantId, 2);
        var order    = Order.CreateFromCart(id, tenantId, null, CartDocument.Empty(), lines);
        lines[0].Ship("T1");
        lines[1].Cancel();
        order.RefreshStatus();
        Assert.Equal(OrderStatus.Shipped, order.Status);
    }

    [Fact]
    public void RefreshStatus_WhenOrderAlreadyCancelled_DoesNotChange()
    {
        var id       = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var lines    = MakeLines(id, tenantId, 2);
        var order    = Order.CreateFromCart(id, tenantId, null, CartDocument.Empty(), lines);
        order.Cancel();
        lines[0].Ship("T1");
        lines[1].Ship("T2");
        order.RefreshStatus(); // should early-return, not override Cancelled
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void CreateFromCart_StartsConfirmed_WithNullFulfillmentTimestamps()
    {
        var order = MakeOrder();
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Null(order.ShippedAt);
        Assert.Null(order.DeliveredAt);
        Assert.Null(order.CancelledAt);
        Assert.Single(order.Lines);
    }

    [Fact]
    public void CreateFromSubscription_HasNullCallRecordId()
    {
        var order = Order.CreateFromSubscription(
            Guid.NewGuid(), Guid.NewGuid(),
            subtotal: 29.95m, shipping: 5.95m, salesTax: 0m, total: 35.90m,
            lines: [OrderLineLifecycleTests.MakeLine()]);
        Assert.Null(order.CallRecordId);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }
}
