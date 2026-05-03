namespace ContactConnection.Domain.CustomFields;

/// <summary>Well-known data type names used in custom field definitions and the data_types seed table.</summary>
public static class CustomFieldDataType
{
    public const string String = "string";
    public const string Integer = "integer";
    public const string Decimal = "decimal";
    public const string Currency = "currency";
    public const string Boolean = "boolean";
    public const string Date = "date";
    public const string DateTime = "datetime";
    public const string Json = "json";

    public static readonly HashSet<string> All =
    [
        String, Integer, Decimal, Currency, Boolean, Date, DateTime, Json
    ];
}
