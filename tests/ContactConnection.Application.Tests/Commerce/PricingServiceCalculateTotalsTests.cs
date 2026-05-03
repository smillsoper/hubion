using ContactConnection.Domain.ValueObjects.Commerce;
using ContactConnection.Infrastructure.Commerce;
using Xunit;

namespace ContactConnection.Application.Tests.Commerce;

public class PricingServiceCalculateTotalsTests
{
    private static readonly PricingService Sut = new(
        new TaxProviderFactory([new FlatRateTaxProvider()]));

    private static CartItem MakeItem(
        decimal fullPrice = 10m,
        int qty = 1,
        decimal shipping = 0m,
        decimal weight = 0m,
        bool taxExempt = false,
        bool shippingExempt = false,
        List<PaymentInstallment>? payments = null,
        List<CartPersonalizationAnswer>? personalization = null) => new(
        OfferId: Guid.NewGuid(),
        ProductId: Guid.NewGuid(),
        Sku: "SKU001",
        Description: "Test Product",
        Quantity: qty,
        FullPrice: fullPrice,
        ExtendedPrice: fullPrice * qty,
        Shipping: shipping,
        Weight: weight,
        SalesTax: 0m,
        ShippingExempt: shippingExempt,
        TaxExempt: taxExempt,
        OnBackOrder: false,
        AutoShip: false,
        AutoShipIntervalDays: 0,
        IsUpsell: false,
        UpsellQty: 0,
        MixMatchCode: null,
        ShipMethod: null,
        DeliveryMessage: null,
        ShipToJson: null,
        Payments: payments ?? [],
        PersonalizationAnswers: personalization ?? [],
        KitSelections: [],
        CanadaSurcharge: 0m,
        AKHISurcharge: 0m,
        OutlyingUSSurcharge: 0m,
        ForeignSurcharge: 0m);

    private static CartDocument MakeCart(
        List<CartItem>? items = null,
        decimal taxRate = 0m,
        bool splitShipping = false,
        bool splitTax = false,
        List<TierRange>? weightTiers = null,
        List<TierRange>? subtotalTiers = null) => new(
        Items: items ?? [],
        ShippingZip: null,
        ShipMethod: null,
        Discount: 0m,
        TaxRate: taxRate,
        TaxProvider: null,
        SplitShippingInPayments: splitShipping,
        SplitSalesTaxInPayments: splitTax,
        ShippingWeightTiers: weightTiers ?? [],
        ShippingSubtotalTiers: subtotalTiers ?? [],
        CartSubtotal: 0m,
        Shipping: 0m,
        SalesTax: 0m,
        PersonalizationCharge: 0m,
        CartTotal: 0m,
        PaymentBreakdowns: []);

    // ── Empty cart ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CalculateTotalsAsync_EmptyCart_ReturnsAllZeros()
    {
        var cart   = MakeCart();
        var result = await Sut.CalculateTotalsAsync(cart);

        Assert.Equal(0m, result.CartSubtotal);
        Assert.Equal(0m, result.Shipping);
        Assert.Equal(0m, result.SalesTax);
        Assert.Equal(0m, result.CartTotal);
        Assert.Empty(result.PaymentBreakdowns);
    }

    // ── Subtotal / shipping / tax ─────────────────────────────────────────────

    [Fact]
    public async Task CalculateTotalsAsync_SingleItem_CorrectSubtotalAndShipping()
    {
        var cart   = MakeCart(items: [MakeItem(fullPrice: 29.95m, qty: 2, shipping: 5.95m)]);
        var result = await Sut.CalculateTotalsAsync(cart);

        Assert.Equal(59.90m, result.CartSubtotal);  // 29.95 * 2
        Assert.Equal(5.95m,  result.Shipping);
        Assert.Equal(0m,     result.SalesTax);
        Assert.Equal(65.85m, result.CartTotal);
    }

    [Fact]
    public async Task CalculateTotalsAsync_TaxRate_AppliedToTaxableItems()
    {
        var items = new List<CartItem>
        {
            MakeItem(fullPrice: 100m, qty: 1, taxExempt: false),
            MakeItem(fullPrice: 50m,  qty: 1, taxExempt: true)  // exempt — excluded from tax base
        };
        var cart   = MakeCart(items: items, taxRate: 0.10m); // 10%
        var result = await Sut.CalculateTotalsAsync(cart);

        Assert.Equal(150m, result.CartSubtotal);
        Assert.Equal(10m,  result.SalesTax);  // 10% of 100 only (50 is exempt)
        Assert.Equal(160m, result.CartTotal);
    }

    [Fact]
    public async Task CalculateTotalsAsync_ShippingExemptItem_ExcludedFromShipping()
    {
        var items = new List<CartItem>
        {
            MakeItem(shipping: 5.95m, shippingExempt: false),
            MakeItem(shipping: 9.95m, shippingExempt: true)  // exempt — excluded
        };
        var cart   = MakeCart(items: items);
        var result = await Sut.CalculateTotalsAsync(cart);

        Assert.Equal(5.95m, result.Shipping); // only the non-exempt item contributes
    }

    // ── Shipping tiers ────────────────────────────────────────────────────────

    [Fact]
    public async Task CalculateTotalsAsync_ShippingWeightTiers_OverrideItemShipping()
    {
        var weightTiers = new List<TierRange>
        {
            new(RangeMin: 0m,  Value: 4.99m),
            new(RangeMin: 5m,  Value: 8.99m),
            new(RangeMin: 10m, Value: 12.99m)
        };
        // Total weight = 2 items * 3 lbs = 6 lbs → hits the 5m tier → $8.99
        var items  = new List<CartItem> { MakeItem(shipping: 99m, weight: 3m, qty: 2) };
        var cart   = MakeCart(items: items, weightTiers: weightTiers);
        var result = await Sut.CalculateTotalsAsync(cart);

        Assert.Equal(8.99m, result.Shipping);
    }

    [Fact]
    public async Task CalculateTotalsAsync_ShippingSubtotalTiers_OverrideItemShipping()
    {
        var subtotalTiers = new List<TierRange>
        {
            new(RangeMin: 0m,   Value: 7.99m),
            new(RangeMin: 50m,  Value: 4.99m),
            new(RangeMin: 100m, Value: 0m)     // free shipping over $100
        };
        var items  = new List<CartItem> { MakeItem(fullPrice: 120m, shipping: 9.99m) };
        var cart   = MakeCart(items: items, subtotalTiers: subtotalTiers);
        var result = await Sut.CalculateTotalsAsync(cart);

        Assert.Equal(0m, result.Shipping); // subtotal $120 → free shipping tier
    }

    [Fact]
    public async Task CalculateTotalsAsync_WeightTiersTakePrecedenceOverSubtotalTiers()
    {
        var weightTiers   = new List<TierRange> { new(RangeMin: 0m, Value: 15m) };
        var subtotalTiers = new List<TierRange> { new(RangeMin: 0m, Value: 5m) };
        var items  = new List<CartItem> { MakeItem(weight: 1m) };
        var cart   = MakeCart(items: items, weightTiers: weightTiers, subtotalTiers: subtotalTiers);
        var result = await Sut.CalculateTotalsAsync(cart);

        Assert.Equal(15m, result.Shipping); // weight tier wins
    }

    // ── Multi-payment breakdowns ──────────────────────────────────────────────

    [Fact]
    public async Task CalculateTotalsAsync_MultiPayment_BuildsBreakdownPerInstallment()
    {
        var payments = new List<PaymentInstallment>
        {
            new(1, "Today",   15m, 0),
            new(2, "30 days", 15m, 30),
            new(3, "60 days", 15m, 60)
        };
        var items  = new List<CartItem> { MakeItem(fullPrice: 45m, qty: 1, payments: payments) };
        var cart   = MakeCart(items: items);
        var result = await Sut.CalculateTotalsAsync(cart);

        Assert.Equal(3, result.PaymentBreakdowns.Count);
        Assert.Equal(1, result.PaymentBreakdowns[0].PaymentNumber);
        Assert.Equal(15m, result.PaymentBreakdowns[0].Subtotal);
        Assert.Equal(15m, result.PaymentBreakdowns[1].Subtotal);
        Assert.Equal(15m, result.PaymentBreakdowns[2].Subtotal);
    }

    [Fact]
    public async Task CalculateTotalsAsync_MultiPayment_ShippingNotSplit_AllInPayment1()
    {
        var payments = new List<PaymentInstallment>
        {
            new(1, "Today",   10m, 0),
            new(2, "30 days", 10m, 30)
        };
        var items  = new List<CartItem> { MakeItem(fullPrice: 20m, shipping: 6m, qty: 1, payments: payments) };
        var cart   = MakeCart(items: items, splitShipping: false);
        var result = await Sut.CalculateTotalsAsync(cart);

        Assert.Equal(6m, result.PaymentBreakdowns[0].Shipping);
        Assert.Equal(0m, result.PaymentBreakdowns[1].Shipping);
    }

    [Fact]
    public async Task CalculateTotalsAsync_MultiPayment_ShippingIsSplit_DistributedEvenly()
    {
        var payments = new List<PaymentInstallment>
        {
            new(1, "Today",   10m, 0),
            new(2, "30 days", 10m, 30)
        };
        var items  = new List<CartItem> { MakeItem(fullPrice: 20m, shipping: 6m, qty: 1, payments: payments) };
        var cart   = MakeCart(items: items, splitShipping: true);
        var result = await Sut.CalculateTotalsAsync(cart);

        Assert.Equal(3m, result.PaymentBreakdowns[0].Shipping);
        Assert.Equal(3m, result.PaymentBreakdowns[1].Shipping);
    }

    [Fact]
    public async Task CalculateTotalsAsync_SinglePayment_BuildsSingleBreakdown()
    {
        var items  = new List<CartItem> { MakeItem(fullPrice: 50m, shipping: 5m) };
        var cart   = MakeCart(items: items);
        var result = await Sut.CalculateTotalsAsync(cart);

        Assert.Single(result.PaymentBreakdowns);
        Assert.Equal(1, result.PaymentBreakdowns[0].PaymentNumber);
        Assert.Equal(50m, result.PaymentBreakdowns[0].Subtotal);
        Assert.Equal(5m,  result.PaymentBreakdowns[0].Shipping);
        Assert.Equal(55m, result.PaymentBreakdowns[0].Total);
    }

    [Fact]
    public async Task CalculateTotalsAsync_RoundSplit_RemainderInPayment1()
    {
        // Shipping = $10 / 3 payments = $3.33 * 3 = $9.99 → remainder $0.01 goes to payment 1
        var payments = new List<PaymentInstallment>
        {
            new(1, "P1", 10m, 0),
            new(2, "P2", 10m, 30),
            new(3, "P3", 10m, 60)
        };
        var items  = new List<CartItem> { MakeItem(fullPrice: 30m, shipping: 10m, payments: payments) };
        var cart   = MakeCart(items: items, splitShipping: true);
        var result = await Sut.CalculateTotalsAsync(cart);

        var p1Shipping = result.PaymentBreakdowns[0].Shipping;
        var p2Shipping = result.PaymentBreakdowns[1].Shipping;
        var p3Shipping = result.PaymentBreakdowns[2].Shipping;

        // Total must equal exactly $10
        Assert.Equal(10m, p1Shipping + p2Shipping + p3Shipping);
        // Remainder goes to payment 1 — it must be >= payments 2 and 3
        Assert.True(p1Shipping >= p2Shipping);
    }
}
