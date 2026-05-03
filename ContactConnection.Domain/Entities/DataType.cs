namespace ContactConnection.Domain.Entities;

/// <summary>
/// Platform-level reference table seeded at startup.
/// Lives in the public schema (ContactConnectionDbContext).
/// Maps type names to CLR/PostgreSQL types and controls which aggregations are valid in reporting.
/// See ARCHITECTURE.md §20.
/// </summary>
public class DataType
{
    public Guid Id { get; private set; }
    public string TypeName { get; private set; } = "";
    public string ClrType { get; private set; } = "";
    public string PostgresType { get; private set; } = "";
    public string? ValidationPattern { get; private set; }
    public string? DisplayFormat { get; private set; }
    public bool IsAggregatable { get; private set; }
    public List<string> AggregationFunctions { get; private set; } = [];

    private DataType() { }

    public static DataType Create(
        Guid id,
        string typeName,
        string clrType,
        string postgresType,
        bool isAggregatable,
        List<string>? aggregationFunctions = null,
        string? validationPattern = null,
        string? displayFormat = null) => new()
    {
        Id = id,
        TypeName = typeName,
        ClrType = clrType,
        PostgresType = postgresType,
        ValidationPattern = validationPattern,
        DisplayFormat = displayFormat,
        IsAggregatable = isAggregatable,
        AggregationFunctions = aggregationFunctions ?? []
    };
}
