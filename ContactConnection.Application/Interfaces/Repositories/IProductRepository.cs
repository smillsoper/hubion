using ContactConnection.Domain.Entities;

namespace ContactConnection.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);

    /// <summary>
    /// Full-text search with optional category + attribute-value facets.
    /// Returns searchable, non-reporting-only products ordered by description.
    /// Multiple attributeValueIds are ANDed — product must have ALL specified values assigned.
    /// </summary>
    Task<List<Product>> SearchAsync(
        string? query,
        Guid? categoryId,
        IReadOnlyList<Guid>? attributeValueIds,
        int page, int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all products sharing a MixMatchCode (used by PricingService during cart total calculation).
    /// </summary>
    Task<List<Product>> GetByMixMatchCodeAsync(string mixMatchCode, CancellationToken ct = default);

    Task AddAsync(Product product, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
