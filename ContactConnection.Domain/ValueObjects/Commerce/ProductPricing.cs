namespace ContactConnection.Domain.ValueObjects.Commerce;

/// <summary>
/// A single installment in a payment plan.
/// Used on Product.Payments (base plan) and within QuantityPriceBreak.Payments (tier-specific plan).
/// IntervalDays = 0 means due today; 30 = due in 30 days, etc.
/// </summary>
public record PaymentInstallment(
    int PaymentNumber,
    string Description,
    decimal Amount,
    int IntervalDays,
    string? PaymentId = null);

/// <summary>
/// A quantity-based price break: when ordered quantity >= MinQty, replace the base
/// payment schedule with this one. Resolution: highest qualifying MinQty wins.
///
/// MixMatchPriceBreaks use the same structure but MinQty is checked against the
/// summed quantity of all cart items sharing the same MixMatchCode — and MixMatch
/// takes priority over QPB when both qualify.
/// </summary>
public record QuantityPriceBreak(
    int MinQty,
    List<PaymentInstallment> Payments);
