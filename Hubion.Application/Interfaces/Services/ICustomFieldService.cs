using Hubion.Domain.Entities;

namespace Hubion.Application.Interfaces.Services;

public record ResolvedCustomField(CustomFieldDefinition Definition, CustomFieldValue? Value);

public interface ICustomFieldService
{
    /// <summary>
    /// Returns the scope-resolved set of custom field definitions for a call record,
    /// each paired with its current value (null if not yet set).
    /// Campaign > client > tenant — most specific definition wins per field name.
    /// </summary>
    Task<List<ResolvedCustomField>> GetFieldsForCallAsync(Guid callRecordId, CancellationToken ct = default);

    /// <summary>Upserts a typed value, then refreshes the call_records.custom_fields snapshot.</summary>
    Task SetValueAsync(Guid callRecordId, Guid definitionId, string rawValue, CancellationToken ct = default);

    /// <summary>Removes a value and refreshes the snapshot.</summary>
    Task DeleteValueAsync(Guid callRecordId, Guid definitionId, CancellationToken ct = default);
}
