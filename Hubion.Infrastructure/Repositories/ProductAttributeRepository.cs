using Hubion.Application.Interfaces.Repositories;
using Hubion.Application.Services;
using Hubion.Domain.Entities;
using Hubion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hubion.Infrastructure.Repositories;

public class ProductAttributeRepository : IProductAttributeRepository
{
    private TenantDbContext? _ctx;
    private readonly ScopedTenantDbContextFactory _factory;

    public ProductAttributeRepository(ScopedTenantDbContextFactory factory)
        => _factory = factory;

    private TenantDbContext Ctx => _ctx ??= _factory.Create();

    public Task<ProductAttribute?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Ctx.ProductAttributes
            .Include(a => a.Values)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<ProductAttributeValue?> GetValueByIdAsync(Guid valueId, CancellationToken ct = default)
        => Ctx.ProductAttributeValues
            .FirstOrDefaultAsync(v => v.Id == valueId, ct);

    public Task<List<ProductAttribute>> GetAllAsync(CancellationToken ct = default)
        => Ctx.ProductAttributes
            .Where(a => a.IsActive)
            .Include(a => a.Values)
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.Name)
            .ToListAsync(ct);

    public async Task AddAsync(ProductAttribute attribute, CancellationToken ct = default)
        => await Ctx.ProductAttributes.AddAsync(attribute, ct);

    public async Task AddValueAsync(ProductAttributeValue value, CancellationToken ct = default)
        => await Ctx.ProductAttributeValues.AddAsync(value, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Ctx.SaveChangesAsync(ct);
}
