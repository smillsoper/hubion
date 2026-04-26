namespace Hubion.Domain.Entities;

/// <summary>
/// Stores the typed value for one custom field on one call record.
/// One populated column per row — the column chosen matches the definition's DataTypeName.
/// Unique constraint: (call_record_id, definition_id).
/// See ARCHITECTURE.md §20.
/// </summary>
public class CustomFieldValue
{
    public Guid Id { get; private set; }
    public Guid CallRecordId { get; private set; }
    public Guid DefinitionId { get; private set; }

    // Typed columns — one populated per row based on data type
    public string? ValueString { get; private set; }
    public long? ValueInteger { get; private set; }
    public decimal? ValueDecimal { get; private set; }
    public bool? ValueBoolean { get; private set; }
    public DateOnly? ValueDate { get; private set; }
    public DateTimeOffset? ValueDatetime { get; private set; }
    public string? ValueJson { get; private set; }   // JSONB stored as string (multiselect, complex types)

    public DateTimeOffset StoredAt { get; private set; }

    // Navigation — loaded when snapshot is rebuilt
    public CustomFieldDefinition? Definition { get; private set; }

    private CustomFieldValue() { }

    public static CustomFieldValue Create(Guid callRecordId, Guid definitionId) => new()
    {
        Id = Guid.NewGuid(),
        CallRecordId = callRecordId,
        DefinitionId = definitionId,
        StoredAt = DateTimeOffset.UtcNow
    };

    public void SetString(string? value) { ClearTypedColumns(); ValueString = value; StoredAt = DateTimeOffset.UtcNow; }
    public void SetInteger(long? value) { ClearTypedColumns(); ValueInteger = value; StoredAt = DateTimeOffset.UtcNow; }
    public void SetDecimal(decimal? value) { ClearTypedColumns(); ValueDecimal = value; StoredAt = DateTimeOffset.UtcNow; }
    public void SetBoolean(bool? value) { ClearTypedColumns(); ValueBoolean = value; StoredAt = DateTimeOffset.UtcNow; }
    public void SetDate(DateOnly? value) { ClearTypedColumns(); ValueDate = value; StoredAt = DateTimeOffset.UtcNow; }
    public void SetDatetime(DateTimeOffset? value) { ClearTypedColumns(); ValueDatetime = value; StoredAt = DateTimeOffset.UtcNow; }
    public void SetJson(string? value) { ClearTypedColumns(); ValueJson = value; StoredAt = DateTimeOffset.UtcNow; }

    /// <summary>Returns the active typed value as an object for snapshot serialization.</summary>
    public object? GetTypedValue() =>
        (object?)ValueString
        ?? ValueInteger
        ?? ValueDecimal
        ?? (object?)ValueBoolean
        ?? ValueDate
        ?? (object?)ValueDatetime
        ?? ValueJson;

    private void ClearTypedColumns()
    {
        ValueString = null;
        ValueInteger = null;
        ValueDecimal = null;
        ValueBoolean = null;
        ValueDate = null;
        ValueDatetime = null;
        ValueJson = null;
    }
}
