using ContactConnection.Domain.Entities;
using ContactConnection.Domain.ValueObjects.Commerce;
using ContactConnection.Infrastructure.Commerce;
using Xunit;

namespace ContactConnection.Application.Tests.Commerce;

public class PricingServiceResolvePaymentsTests
{
    private static readonly PricingService Sut = new(
        new TaxProviderFactory([new FlatRateTaxProvider()]));

    private static Offer MakeOffer(
        decimal price = 29.95m,
        List<PaymentInstallment>? payments = null,
        List<QuantityPriceBreak>? qpb = null,
        List<QuantityPriceBreak>? mixMatch = null,
        string? mixMatchCode = null)
    {
        var offer = Offer.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Offer", price);
        offer.SetPricing(price, payments ?? [new(1, "Single Pay", price, 0)], qpb, mixMatch);
        if (mixMatchCode is not null)
            offer.SetMixMatch(mixMatchCode, mixMatch);
        return offer;
    }

    private static CartItem MakeCartItem(
        int qty = 1,
        string? mixMatchCode = null,
        List<PaymentInstallment>? payments = null) => new(
        OfferId: Guid.NewGuid(),
        ProductId: Guid.NewGuid(),
        Sku: "SKU001",
        Description: "Test",
        Quantity: qty,
        FullPrice: 29.95m,
        ExtendedPrice: 29.95m * qty,
        Shipping: 0m,
        Weight: 0m,
        SalesTax: 0m,
        ShippingExempt: false,
        TaxExempt: false,
        OnBackOrder: false,
        AutoShip: false,
        AutoShipIntervalDays: 0,
        IsUpsell: false,
        UpsellQty: 0,
        MixMatchCode: mixMatchCode,
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

    // ── Base schedule fallback ────────────────────────────────────────────────

    [Fact]
    public void ResolvePayments_NoBreaks_ReturnsBasePayments()
    {
        var basePay = new List<PaymentInstallment>
        {
            new(1, "Payment 1", 10m, 0),
            new(2, "Payment 2", 10m, 30)
        };
        var offer = MakeOffer(payments: basePay);

        var result = Sut.ResolvePayments(offer, quantity: 1, allCartItems: []);

        Assert.Equal(basePay, result);
    }

    // ── QPB ───────────────────────────────────────────────────────────────────

    [Fact]
    public void ResolvePayments_QPB_QtyMeetsThreshold_ReturnsQPBPayments()
    {
        var basePay = new List<PaymentInstallment> { new(1, "Base", 29.95m, 0) };
        var qpbPay  = new List<PaymentInstallment> { new(1, "QPB", 24.95m, 0) };
        var qpb     = new List<QuantityPriceBreak> { new(MinQty: 2, Payments: qpbPay) };
        var offer   = MakeOffer(payments: basePay, qpb: qpb);

        var result = Sut.ResolvePayments(offer, quantity: 2, allCartItems: []);

        Assert.Equal(qpbPay, result);
    }

    [Fact]
    public void ResolvePayments_QPB_QtyBelowThreshold_ReturnsBase()
    {
        var basePay = new List<PaymentInstallment> { new(1, "Base", 29.95m, 0) };
        var qpbPay  = new List<PaymentInstallment> { new(1, "QPB", 24.95m, 0) };
        var qpb     = new List<QuantityPriceBreak> { new(MinQty: 2, Payments: qpbPay) };
        var offer   = MakeOffer(payments: basePay, qpb: qpb);

        var result = Sut.ResolvePayments(offer, quantity: 1, allCartItems: []);

        Assert.Equal(basePay, result);
    }

    [Fact]
    public void ResolvePayments_QPB_BestBreakWins_HighestQualifyingMinQty()
    {
        var pay2 = new List<PaymentInstallment> { new(1, "2+", 24.95m, 0) };
        var pay5 = new List<PaymentInstallment> { new(1, "5+", 19.95m, 0) };
        var qpb  = new List<QuantityPriceBreak>
        {
            new(MinQty: 2, Payments: pay2),
            new(MinQty: 5, Payments: pay5)
        };
        var offer = MakeOffer(qpb: qpb);

        var result = Sut.ResolvePayments(offer, quantity: 6, allCartItems: []);

        Assert.Equal(pay5, result); // highest qualifying break (5+) wins
    }

    // ── MixMatch ──────────────────────────────────────────────────────────────

    [Fact]
    public void ResolvePayments_MixMatch_GroupQtyMeetsThreshold_ReturnsMixMatchPayments()
    {
        var basePay  = new List<PaymentInstallment> { new(1, "Base", 29.95m, 0) };
        var mmPay    = new List<PaymentInstallment> { new(1, "MM",   24.95m, 0) };
        var mmBreaks = new List<QuantityPriceBreak> { new(MinQty: 3, Payments: mmPay) };
        var offer    = MakeOffer(payments: basePay, mixMatch: mmBreaks, mixMatchCode: "GROUP1");

        // Two existing cart items with same MixMatchCode (qty=1 each) + the new item (qty=1) = 3
        var cartItems = new List<CartItem>
        {
            MakeCartItem(qty: 1, mixMatchCode: "GROUP1"),
            MakeCartItem(qty: 1, mixMatchCode: "GROUP1")
        };

        var result = Sut.ResolvePayments(offer, quantity: 1, allCartItems: cartItems);

        Assert.Equal(mmPay, result);
    }

    [Fact]
    public void ResolvePayments_MixMatch_GroupQtyInsufficient_ReturnsBase()
    {
        var basePay  = new List<PaymentInstallment> { new(1, "Base", 29.95m, 0) };
        var mmPay    = new List<PaymentInstallment> { new(1, "MM",   24.95m, 0) };
        var mmBreaks = new List<QuantityPriceBreak> { new(MinQty: 5, Payments: mmPay) };
        var offer    = MakeOffer(payments: basePay, mixMatch: mmBreaks, mixMatchCode: "GROUP1");

        // Only 1 existing item (qty=1) + new item (qty=1) = 2; threshold is 5
        var cartItems = new List<CartItem> { MakeCartItem(qty: 1, mixMatchCode: "GROUP1") };

        var result = Sut.ResolvePayments(offer, quantity: 1, allCartItems: cartItems);

        Assert.Equal(basePay, result);
    }

    [Fact]
    public void ResolvePayments_MixMatch_TakesPriorityOverQPB()
    {
        var mmPay    = new List<PaymentInstallment> { new(1, "MM",  24.95m, 0) };
        var qpbPay   = new List<PaymentInstallment> { new(1, "QPB", 22.95m, 0) };
        var mmBreaks = new List<QuantityPriceBreak> { new(MinQty: 2, Payments: mmPay) };
        var qpb      = new List<QuantityPriceBreak> { new(MinQty: 2, Payments: qpbPay) };
        var offer    = MakeOffer(mixMatch: mmBreaks, qpb: qpb, mixMatchCode: "GROUP1");

        // Both MixMatch (group qty=2) and QPB (item qty=2) would qualify; MixMatch wins
        var cartItems = new List<CartItem> { MakeCartItem(qty: 1, mixMatchCode: "GROUP1") };
        var result = Sut.ResolvePayments(offer, quantity: 1, allCartItems: cartItems);

        Assert.Equal(mmPay, result);
    }

    [Fact]
    public void ResolvePayments_MixMatch_DifferentCode_NotCounted()
    {
        var basePay  = new List<PaymentInstallment> { new(1, "Base", 29.95m, 0) };
        var mmPay    = new List<PaymentInstallment> { new(1, "MM",   24.95m, 0) };
        var mmBreaks = new List<QuantityPriceBreak> { new(MinQty: 3, Payments: mmPay) };
        var offer    = MakeOffer(payments: basePay, mixMatch: mmBreaks, mixMatchCode: "GROUP1");

        // Cart items have a DIFFERENT mix-match code — should not count toward GROUP1
        var cartItems = new List<CartItem>
        {
            MakeCartItem(qty: 5, mixMatchCode: "GROUP2"),
            MakeCartItem(qty: 5, mixMatchCode: "GROUP2")
        };

        var result = Sut.ResolvePayments(offer, quantity: 1, allCartItems: cartItems);

        Assert.Equal(basePay, result); // MixMatch not triggered for wrong group
    }
}
