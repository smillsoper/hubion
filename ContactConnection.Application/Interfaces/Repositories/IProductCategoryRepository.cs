using ContactConnection.Domain.Entities;

namespace ContactConnection.Application.Interfaces.Repositories;

public interface IProductCategoryRepository
{
    Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ProductCategory>> GetRootsAsync(CancellationToken ct = default);
    Task<List<ProductCategory>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);
    Task AddAsync(ProductCategory category, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
