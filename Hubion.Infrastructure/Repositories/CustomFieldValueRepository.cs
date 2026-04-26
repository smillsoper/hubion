using Hubion.Application.Interfaces.Repositories;
using Hubion.Application.Services;
using Hubion.Domain.Entities;
using Hubion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hubion.Infrastructure.Repositories;

public class CustomFieldValueRepository : ICustomFieldValueRepository
{
    private TenantDbContext? _ctx;
    private readonly ScopedTenantDbContextFactory _factory;

    public CustomFieldValueRepository(ScopedTenantDbContextFactory factory)
        => _factory = factory;

    private TenantDbContext Ctx => _ctx ??= _factory.Create();

    public Task<List<CustomFieldValue>> GetByCallRecordAsync(Guid callRecordId, CancellationToken ct = default)
        => Ctx.CustomFieldValues
            .Include(v => v.Definition)
            .Where(v => v.CallRecordId == callRecordId)
            .ToListAsync(ct);

    public Task<CustomFieldValue?> GetByCallRecordAndDefinitionAsync(
        Guid callRecordId,
        Guid definitionId,
        CancellationToken ct = default)
        => Ctx.CustomFieldValues
            .FirstOrDefaultAsync(v => v.CallRecordId == callRecordId && v.DefinitionId == definitionId, ct);

    public async Task AddAsync(CustomFieldValue value, CancellationToken ct = default)
        => await Ctx.CustomFieldValues.AddAsync(value, ct);

    public async Task DeleteAsync(Guid callRecordId, Guid definitionId, CancellationToken ct = default)
    {
        var existing = await GetByCallRecordAndDefinitionAsync(callRecordId, definitionId, ct);
        if (existing != null)
            Ctx.CustomFieldValues.Remove(existing);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Ctx.SaveChangesAsync(ct);
}
