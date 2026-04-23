namespace Hubion.Application.Interfaces.Services;

/// <summary>
/// Resolves the correct ITaxProvider implementation for a given provider key.
/// Registered as a singleton; providers register themselves by ProviderKey.
/// </summary>
public interface ITaxProviderFactory
{
    /// <summary>
    /// Returns the ITaxProvider for the given key, or the FlatRateTaxProvider
    /// if the key is null, empty, or unrecognised.
    /// </summary>
    ITaxProvider Resolve(string? providerKey);
}
