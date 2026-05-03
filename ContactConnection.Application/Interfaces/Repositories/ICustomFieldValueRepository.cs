using ContactConnection.Domain.Entities;

namespace ContactConnection.Application.Interfaces.Repositories;

public interface ICustomFieldValueRepository
{
    /// <summary>Returns all values for a call record, including their definitions.</summary>
    Task<List<CustomFieldValue>> GetByCallRecordAsync(Guid callRecordId, CancellationToken ct = default);

    Task<CustomFieldValue?> GetByCallRecordAndDefinitionAsync(
        Guid callRecordId,
        Guid definitionId,
        CancellationToken ct = default);

    Task AddAsync(CustomFieldValue value, CancellationToken ct = default);
    Task DeleteAsync(Guid callRecordId, Guid definitionId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
