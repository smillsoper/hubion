using Hubion.Domain.Entities;

namespace Hubion.Application.Interfaces.Repositories;

public interface ICustomFieldDefinitionRepository
{
    Task<CustomFieldDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all active definitions that could apply to a given tenant/client/campaign context.</summary>
    Task<List<CustomFieldDefinition>> GetForContextAsync(
        Guid tenantId,
        Guid? clientId = null,
        Guid? campaignId = null,
        CancellationToken ct = default);

    Task<List<CustomFieldDefinition>> GetAllForTenantAsync(Guid tenantId, CancellationToken ct = default);

    Task AddAsync(CustomFieldDefinition definition, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
