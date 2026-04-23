using Hubion.Application.Interfaces.Services;
using Hubion.Domain.ValueObjects.Commerce;

namespace Hubion.Infrastructure.Commerce;

/// <summary>
/// Default tax provider — applies a single flat rate to the taxable subtotal.
/// Rate comes from CartDocument.TaxRate (set by the caller at cart creation time).
/// No external calls; always synchronous.
/// </summary>
public class FlatRateTaxProvider : ITaxProvider
{
    public string ProviderKey => "";  // default provider — selected when TaxProvider is null or empty

    public Task<TaxResult> CalculateTaxAsync(CartDocument cart, CancellationToken ct = default)
    {
        var taxableSubtotal = cart.Items
            .Where(i => !i.TaxExempt)
            .Sum(i => i.ExtendedPrice + i.PersonalizationAnswers.Sum(a => a.ChargeAmount) * i.Quantity);

        var taxAmount = Math.Round(taxableSubtotal * cart.TaxRate, 2);

        return Task.FromResult(new TaxResult(
            Rate:          cart.TaxRate,
            TaxAmount:     taxAmount,
            Jurisdictions: null));   // flat rate has no per-jurisdiction breakdown
    }
}
