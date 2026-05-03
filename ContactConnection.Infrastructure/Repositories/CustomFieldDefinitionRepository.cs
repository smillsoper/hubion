using ContactConnection.Application.Interfaces.Repositories;
using ContactConnection.Application.Services;
using ContactConnection.Domain.Entities;
using ContactConnection.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactConnection.Infrastructure.Repositories;

public class CustomFieldDefinitionRepository : ICustomFieldDefinitionRepository
{
    private TenantDbContext? _ctx;
    private readonly ScopedTenantDbContextFactory _factory;

    public CustomFieldDefinitionRepository(ScopedTenantDbContextFactory factory)
        => _factory = factory;

    private TenantDbContext Ctx => _ctx ??= _factory.Create();

    public Task<CustomFieldDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Ctx.CustomFieldDefinitions.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<List<CustomFieldDefinition>> GetForContextAsync(
        Guid tenantId,
        Guid? clientId = null,
        Guid? campaignId = null,
        CancellationToken ct = default)
        => Ctx.CustomFieldDefinitions
            .Where(d =>
                d.TenantId == tenantId &&
                d.IsActive &&
                (d.ClientId == null || d.ClientId == clientId) &&
                (d.CampaignId == null || d.CampaignId == campaignId))
            .OrderBy(d => d.DisplayOrder)
            .ToListAsync(ct);

    public Task<List<CustomFieldDefinition>> GetAllForTenantAsync(Guid tenantId, CancellationToken ct = default)
        => Ctx.CustomFieldDefinitions
            .Where(d => d.TenantId == tenantId)
            .OrderBy(d => d.DisplayOrder)
            .ToListAsync(ct);

    public async Task AddAsync(CustomFieldDefinition definition, CancellationToken ct = default)
        => await Ctx.CustomFieldDefinitions.AddAsync(definition, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Ctx.SaveChangesAsync(ct);
}
