using ContactConnection.Application.Interfaces.Services;
using ContactConnection.Domain.Entities;
using ContactConnection.Domain.ValueObjects.Commerce;

namespace ContactConnection.Infrastructure.Commerce;

/// <summary>
/// Resolves payment schedules and calculates cart totals.
///
/// Price break resolution (ResolvePayments):
///   1. MixMatch — sum qty across all cart items sharing the same MixMatchCode;
///      find the highest qualifying MixMatchPriceBreak on the Offer. Applies to all items in the group.
///   2. QPB      — item's own qty; find the highest qualifying QuantityPriceBreak.
///   3. Fallback — Offer.Payments (base schedule).
///
/// Tax calculation is delegated to ITaxProviderFactory, which resolves the correct
/// ITaxProvider from CartDocument.TaxProvider (null/empty = FlatRateTaxProvider).
/// </summary>
public class PricingService : IPricingService
{
    private readonly ITaxProviderFactory _taxProviderFactory;

    public PricingService(ITaxProviderFactory taxProviderFactory)
        => _taxProviderFactory = taxProviderFactory;

    public List<PaymentInstallment> ResolvePayments(
        Offer offer,
        int quantity,
        IReadOnlyList<CartItem> allCartItems)
    {
        // 1. MixMatch
        if (!string.IsNullOrEmpty(offer.MixMatchCode) && offer.MixMatchPriceBreaks.Count > 0)
        {
            var groupQty = allCartItems
                .Where(i => i.MixMatchCode == offer.MixMatchCode)
                .Sum(i => i.Quantity)
                + quantity;  // include the item being priced

            var mmBreak = FindBestBreak(offer.MixMatchPriceBreaks, groupQty);
            if (mmBreak is not null)
                return mmBreak.Payments;
        }

        // 2. QPB
        if (offer.QuantityPriceBreaks.Count > 0)
        {
            var qpbBreak = FindBestBreak(offer.QuantityPriceBreaks, quantity);
            if (qpbBreak is not null)
                return qpbBreak.Payments;
        }

        // 3. Base schedule
        return offer.Payments;
    }

    public async Task<CartDocument> CalculateTotalsAsync(CartDocument cart, CancellationToken ct = default)
    {
        if (cart.Items.Count == 0)
            return cart with
            {
                CartSubtotal          = 0,
                Shipping              = 0,
                SalesTax              = 0,
                PersonalizationCharge = 0,
                CartTotal             = 0,
                PaymentBreakdowns     = []
            };

        // ── Per-item totals ─────────────────────────────────────────────────

        decimal cartSubtotal  = 0;
        decimal totalShipping = 0;
        decimal persCharge    = 0;

        foreach (var item in cart.Items)
        {
            cartSubtotal += item.ExtendedPrice;

            if (!item.ShippingExempt)
                totalShipping += item.Shipping;

            persCharge += item.PersonalizationAnswers.Sum(a => a.ChargeAmount) * item.Quantity;
        }

        // ── Shipping tier override ──────────────────────────────────────────
        // Weight tiers take precedence when both weight and subtotal tiers are configured.

        if (cart.ShippingWeightTiers.Count > 0)
        {
            var totalWeight = cart.Items.Where(i => !i.ShippingExempt).Sum(i => i.Weight * i.Quantity);
            var tierShipping = ResolveTier(cart.ShippingWeightTiers, totalWeight);
            if (tierShipping.HasValue) totalShipping = tierShipping.Value;
        }
        else if (cart.ShippingSubtotalTiers.Count > 0)
        {
            var tierShipping = ResolveTier(cart.ShippingSubtotalTiers, cartSubtotal);
            if (tierShipping.HasValue) totalShipping = tierShipping.Value;
        }

        // ── Tax — delegated to the configured provider ──────────────────────

        var taxProvider = _taxProviderFactory.Resolve(cart.TaxProvider);
        var taxResult   = await taxProvider.CalculateTaxAsync(cart, ct);
        var salesTax    = taxResult.TaxAmount;

        // ── Discount ────────────────────────────────────────────────────────

        var discount  = cart.Discount;
        var cartTotal = cartSubtotal + totalShipping + salesTax + persCharge - discount;

        // ── Payment installment breakdown ───────────────────────────────────

        var breakdowns = BuildPaymentBreakdowns(
            cart.Items, cartSubtotal, totalShipping, salesTax,
            discount, persCharge, cart.SplitShippingInPayments, cart.SplitSalesTaxInPayments);

        return cart with
        {
            CartSubtotal          = cartSubtotal,
            Shipping              = totalShipping,
            SalesTax              = salesTax,
            PersonalizationCharge = persCharge,
            CartTotal             = cartTotal,
            PaymentBreakdowns     = breakdowns
        };
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static QuantityPriceBreak? FindBestBreak(
        IReadOnlyList<QuantityPriceBreak> breaks, int qty)
        => breaks
            .Where(b => b.MinQty <= qty)
            .MaxBy(b => b.MinQty);

    private static decimal? ResolveTier(IReadOnlyList<TierRange> tiers, decimal value)
    {
        var tier = tiers
            .Where(t => t.RangeMin <= value)
            .MaxBy(t => t.RangeMin);
        return tier?.Value;
    }

    private static List<CartPaymentBreakdown> BuildPaymentBreakdowns(
        IReadOnlyList<CartItem> items,
        decimal cartSubtotal, decimal shipping, decimal salesTax,
        decimal discount, decimal persCharge,
        bool splitShipping, bool splitTax)
    {
        var maxPayments = items
            .Where(i => i.Payments.Count > 0)
            .Select(i => i.Payments.Count)
            .DefaultIfEmpty(1)
            .Max();

        if (maxPayments <= 1)
        {
            return [new CartPaymentBreakdown(
                PaymentNumber:         1,
                Subtotal:              cartSubtotal,
                Shipping:              shipping,
                SalesTax:              salesTax,
                Discount:              discount,
                PersonalizationCharge: persCharge,
                Total:                 cartSubtotal + shipping + salesTax + persCharge - discount)];
        }

        var breakdowns = new List<CartPaymentBreakdown>(maxPayments);

        for (var pmtNum = 1; pmtNum <= maxPayments; pmtNum++)
        {
            var pmtSubtotal = items.Sum(item =>
            {
                var pmt = item.Payments.FirstOrDefault(p => p.PaymentNumber == pmtNum);
                return pmt is null ? 0 : pmt.Amount * item.Quantity;
            });

            // Not splitting: full amount in payment 1, zero in all others.
            // Splitting: divide evenly across all payments (rounding remainder lands in payment 1).
            var pmtShipping = !splitShipping ? (pmtNum == 1 ? shipping  : 0)
                                             : RoundSplit(shipping,  pmtNum, maxPayments);
            var pmtTax      = !splitTax      ? (pmtNum == 1 ? salesTax : 0)
                                             : RoundSplit(salesTax, pmtNum, maxPayments);
            var pmtDiscount = pmtNum == 1 ? discount   : 0;
            var pmtPers     = pmtNum == 1 ? persCharge : 0;

            breakdowns.Add(new CartPaymentBreakdown(
                PaymentNumber:         pmtNum,
                Subtotal:              pmtSubtotal,
                Shipping:              pmtShipping,
                SalesTax:              pmtTax,
                Discount:              pmtDiscount,
                PersonalizationCharge: pmtPers,
                Total:                 pmtSubtotal + pmtShipping + pmtTax + pmtPers - pmtDiscount));
        }

        return breakdowns;
    }

    /// <summary>
    /// Evenly distributes a decimal amount across N installments,
    /// placing any rounding remainder in the first installment.
    /// </summary>
    private static decimal RoundSplit(decimal amount, int paymentNumber, int totalPayments)
    {
        var perInstallment = Math.Round(amount / totalPayments, 2);
        if (paymentNumber == 1)
        {
            var remainder = amount - perInstallment * totalPayments;
            return perInstallment + remainder;
        }
        return perInstallment;
    }
}
