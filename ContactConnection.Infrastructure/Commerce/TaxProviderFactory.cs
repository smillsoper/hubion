using ContactConnection.Application.Interfaces.Services;

namespace ContactConnection.Infrastructure.Commerce;

/// <summary>
/// Resolves the correct ITaxProvider by ProviderKey.
/// Receives all registered ITaxProvider implementations via DI enumeration.
/// Falls back to FlatRateTaxProvider when the key is null, empty, or unrecognised.
/// </summary>
public class TaxProviderFactory : ITaxProviderFactory
{
    private readonly Dictionary<string, ITaxProvider> _providers;
    private readonly ITaxProvider _default;

    public TaxProviderFactory(IEnumerable<ITaxProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.ProviderKey, StringComparer.OrdinalIgnoreCase);

        if (!_providers.TryGetValue("", out _default!))
            throw new InvalidOperationException(
                "No default tax provider (ProviderKey = \"\") is registered. " +
                "Register FlatRateTaxProvider in DI.");
    }

    public ITaxProvider Resolve(string? providerKey)
    {
        if (string.IsNullOrEmpty(providerKey))
            return _default;

        return _providers.TryGetValue(providerKey, out var provider)
            ? provider
            : _default;
    }
}
