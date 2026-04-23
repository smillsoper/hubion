using Hubion.Domain.Entities;

namespace Hubion.Application.Interfaces.Repositories;

public interface IProductAttributeRepository
{
    Task<ProductAttribute?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductAttributeValue?> GetValueByIdAsync(Guid valueId, CancellationToken ct = default);
    Task<List<ProductAttribute>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ProductAttribute attribute, CancellationToken ct = default);
    Task AddValueAsync(ProductAttributeValue value, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
