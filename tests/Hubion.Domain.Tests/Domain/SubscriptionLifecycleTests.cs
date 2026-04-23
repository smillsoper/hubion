using System.Reflection;
using Hubion.Domain.Entities;
using Xunit;

namespace Hubion.Domain.Tests.Domain;

public class SubscriptionLifecycleTests
{
    internal static Subscription MakeSubscription(int intervalDays = 30)
    {
        var tenantId = Guid.NewGuid();
        var orderId  = Guid.NewGuid();
        var line     = OrderLineLifecycleTests.MakeLine(autoShip: true, intervalDays: intervalDays);
        return Subscription.CreateFromOrderLine(tenantId, null, orderId, line);
    }

    private static void SetNextShipDate(Subscription sub, DateTimeOffset date)
    {
        typeof(Subscription)
            .GetField("<NextShipDate>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(sub, date);
    }

    // ── IsDue ─────────────────────────────────────────────────────────────────

    [Fact]
    public void IsDue_Active_FutureDate_ReturnsFalse()
    {
        var sub = MakeSubscription(intervalDays: 30);
        // NextShipDate = now + 30 days (in the future)
        Assert.False(sub.IsDue());
    }

    [Fact]
    public void IsDue_Active_PastDate_ReturnsTrue()
    {
        var sub = MakeSubscription();
        SetNextShipDate(sub, DateTimeOffset.UtcNow.AddDays(-1));
        Assert.True(sub.IsDue());
    }

    [Fact]
    public void IsDue_Paused_ReturnsFalse_EvenWhenOverdue()
    {
        var sub = MakeSubscription();
        SetNextShipDate(sub, DateTimeOffset.UtcNow.AddDays(-1));
        sub.Pause();
        Assert.False(sub.IsDue());
    }

    [Fact]
    public void IsDue_Cancelled_ReturnsFalse_EvenWhenOverdue()
    {
        var sub = MakeSubscription();
        SetNextShipDate(sub, DateTimeOffset.UtcNow.AddDays(-1));
        sub.Cancel();
        Assert.False(sub.IsDue());
    }

    // ── RecordShipment ────────────────────────────────────────────────────────

    [Fact]
    public void RecordShipment_IncrementsShipmentCount()
    {
        var sub = MakeSubscription(intervalDays: 30);
        Assert.Equal(0, sub.ShipmentCount);
        sub.RecordShipment();
        Assert.Equal(1, sub.ShipmentCount);
        sub.RecordShipment();
        Assert.Equal(2, sub.ShipmentCount);
    }

    [Fact]
    public void RecordShipment_SetsLastShipDate()
    {
        var before = DateTimeOffset.UtcNow;
        var sub = MakeSubscription();
        sub.RecordShipment();
        Assert.NotNull(sub.LastShipDate);
        Assert.True(sub.LastShipDate >= before);
    }

    [Fact]
    public void RecordShipment_AdvancesNextShipDate_ByIntervalDays()
    {
        var sub = MakeSubscription(intervalDays: 30);
        var before = DateTimeOffset.UtcNow;
        sub.RecordShipment();
        // NextShipDate should be approximately now + 30 days
        var expected = before.AddDays(30);
        Assert.True(sub.NextShipDate >= expected.AddSeconds(-5));
        Assert.True(sub.NextShipDate <= expected.AddSeconds(5));
    }

    [Fact]
    public void RecordShipment_MakesSubscriptionNotDueAgain()
    {
        var sub = MakeSubscription(intervalDays: 30);
        SetNextShipDate(sub, DateTimeOffset.UtcNow.AddDays(-1));
        Assert.True(sub.IsDue());
        sub.RecordShipment();
        Assert.False(sub.IsDue()); // NextShipDate now 30 days in the future
    }

    // ── Pause / Resume ────────────────────────────────────────────────────────

    [Fact]
    public void Pause_SetsPausedStatus()
    {
        var sub = MakeSubscription();
        sub.Pause();
        Assert.Equal(SubscriptionStatus.Paused, sub.Status);
    }

    [Fact]
    public void Resume_AfterPause_SetsActiveStatus()
    {
        var sub = MakeSubscription();
        sub.Pause();
        sub.Resume();
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_SetsCancelledStatus()
    {
        var sub = MakeSubscription();
        sub.Cancel();
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
    }

    [Fact]
    public void Cancel_SetsCancelledAt()
    {
        var before = DateTimeOffset.UtcNow;
        var sub = MakeSubscription();
        sub.Cancel();
        Assert.NotNull(sub.CancelledAt);
        Assert.True(sub.CancelledAt >= before);
    }

    [Fact]
    public void NewSubscription_StartsActive_WithZeroShipmentCount()
    {
        var sub = MakeSubscription();
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
        Assert.Equal(0, sub.ShipmentCount);
        Assert.Null(sub.LastShipDate);
        Assert.Null(sub.CancelledAt);
    }
}
