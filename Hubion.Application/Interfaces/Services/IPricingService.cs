using Hubion.Domain.Entities;
using Hubion.Domain.ValueObjects.Commerce;

namespace Hubion.Application.Interfaces.Services;

public interface IPricingService
{
    /// <summary>
    /// Resolve the payment schedule for a single cart item using the Offer's pricing configuration.
    ///
    /// Resolution order (first match wins):
    ///   1. MixMatch — sum qty of all cart items sharing the same MixMatchCode;
    ///      find the highest qualifying MixMatchPriceBreak on the offer.
    ///   2. QPB      — item's own qty; find the highest qualifying QuantityPriceBreak.
    ///   3. Fallback — Offer.Payments (base schedule).
    /// </summary>
    List<PaymentInstallment> ResolvePayments(
        Offer offer,
        int quantity,
        IReadOnlyList<CartItem> allCartItems);

    /// <summary>
    /// Recalculate all cart totals in one pass:
    ///   - Extended prices per item (resolved price × qty)
    ///   - Shipping via weight tiers, then subtotal tiers (weight tiers take precedence if both are set)
    ///   - Geographic surcharges based on ShippingZip classification
    ///   - Sales tax — delegated to ITaxProvider resolved from CartDocument.TaxProvider key
    ///   - Personalization charges
    ///   - Payment installment breakdown (optionally splitting shipping and tax across installments)
    ///
    /// Returns a new CartDocument with all computed fields set. Does not mutate the input.
    /// Async because external tax providers (Avalara, TaxJar) require HTTP calls.
    /// </summary>
    Task<CartDocument> CalculateTotalsAsync(CartDocument cart, CancellationToken ct = default);
}
