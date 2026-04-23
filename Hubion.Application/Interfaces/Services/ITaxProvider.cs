using Hubion.Domain.ValueObjects.Commerce;

namespace Hubion.Application.Interfaces.Services;

/// <summary>
/// Calculates sales tax for a cart.
///
/// The active provider is determined by CartDocument.TaxProvider:
///   null / ""     → FlatRateTaxProvider (uses CartDocument.TaxRate directly)
///   "avalara"     → Avalara AvaTax (future)
///   "taxjar"      → TaxJar (future)
///
/// New providers are registered in DI as named implementations via ITaxProviderFactory.
/// Adding a new provider never requires changes to PricingService.
/// </summary>
public interface ITaxProvider
{
    /// <summary>The provider identifier that selects this implementation (e.g. "avalara").</summary>
    string ProviderKey { get; }

    /// <summary>
    /// Calculate tax for the given cart. Returns a TaxResult containing the effective
    /// rate and an optional per-jurisdiction breakdown for display/audit purposes.
    /// Does not mutate the cart.
    /// </summary>
    Task<TaxResult> CalculateTaxAsync(CartDocument cart, CancellationToken ct = default);
}

/// <summary>
/// The result of a tax calculation. Rate is the blended effective rate applied
/// to the taxable subtotal. Jurisdictions is an optional breakdown for display.
/// </summary>
public record TaxResult(
    decimal Rate,
    decimal TaxAmount,
    List<JurisdictionTax>? Jurisdictions = null);

/// <summary>Per-jurisdiction tax line for multi-state nexus display.</summary>
public record JurisdictionTax(
    string Jurisdiction,
    string TaxType,         // "state" | "county" | "city" | "special"
    decimal Rate,
    decimal Amount);
