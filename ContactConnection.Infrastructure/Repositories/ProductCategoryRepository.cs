using ContactConnection.Application.Interfaces.Repositories;
using ContactConnection.Application.Services;
using ContactConnection.Domain.Entities;
using ContactConnection.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactConnection.Infrastructure.Repositories;

public class ProductCategoryRepository : IProductCategoryRepository
{
    private TenantDbContext? _ctx;
    private readonly ScopedTenantDbContextFactory _factory;

    public ProductCategoryRepository(ScopedTenantDbContextFactory factory)
        => _factory = factory;

    private TenantDbContext Ctx => _ctx ??= _factory.Create();

    public Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Ctx.ProductCategories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<ProductCategory>> GetRootsAsync(CancellationToken ct = default)
        => Ctx.ProductCategories
            .Where(c => c.ParentId == null && c.IsActive)
            .Include(c => c.Children)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

    public Task<List<ProductCategory>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
        => Ctx.ProductCategories
            .Where(c => c.ParentId == parentId && c.IsActive)
            .Include(c => c.Children)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

    public async Task AddAsync(ProductCategory category, CancellationToken ct = default)
        => await Ctx.ProductCategories.AddAsync(category, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Ctx.SaveChangesAsync(ct);
}
