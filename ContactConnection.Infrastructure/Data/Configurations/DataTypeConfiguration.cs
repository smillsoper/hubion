using ContactConnection.Domain.CustomFields;
using ContactConnection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactConnection.Infrastructure.Data.Configurations;

public class DataTypeConfiguration : IEntityTypeConfiguration<DataType>
{
    // Stable well-known IDs — never change these after seeding
    public static readonly Guid StringId   = new("10000000-0000-0000-0000-000000000001");
    public static readonly Guid IntegerId  = new("10000000-0000-0000-0000-000000000002");
    public static readonly Guid DecimalId  = new("10000000-0000-0000-0000-000000000003");
    public static readonly Guid CurrencyId = new("10000000-0000-0000-0000-000000000004");
    public static readonly Guid BooleanId  = new("10000000-0000-0000-0000-000000000005");
    public static readonly Guid DateId     = new("10000000-0000-0000-0000-000000000006");
    public static readonly Guid DateTimeId = new("10000000-0000-0000-0000-000000000007");
    public static readonly Guid JsonId     = new("10000000-0000-0000-0000-000000000008");

    public void Configure(EntityTypeBuilder<DataType> builder)
    {
        builder.ToTable("data_types");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.TypeName).HasColumnName("type_name").HasMaxLength(50).IsRequired();
        builder.Property(d => d.ClrType).HasColumnName("clr_type").HasMaxLength(100).IsRequired();
        builder.Property(d => d.PostgresType).HasColumnName("postgres_type").HasMaxLength(50).IsRequired();
        builder.Property(d => d.ValidationPattern).HasColumnName("validation_pattern");
        builder.Property(d => d.DisplayFormat).HasColumnName("display_format");
        builder.Property(d => d.IsAggregatable).HasColumnName("is_aggregatable");
        builder.Property(d => d.AggregationFunctions)
            .HasColumnName("aggregation_functions")
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasDefaultValueSql("'[]'::jsonb");

        builder.HasIndex(d => d.TypeName).IsUnique().HasDatabaseName("ix_data_types_type_name");

        builder.HasData(
            DataType.Create(StringId,   CustomFieldDataType.String,   "System.String",         "text",            false, []),
            DataType.Create(IntegerId,  CustomFieldDataType.Integer,  "System.Int64",           "bigint",          true,  ["sum", "min", "max", "avg", "count"]),
            DataType.Create(DecimalId,  CustomFieldDataType.Decimal,  "System.Decimal",         "numeric(18,6)",   true,  ["sum", "min", "max", "avg", "count"]),
            DataType.Create(CurrencyId, CustomFieldDataType.Currency, "System.Decimal",         "numeric(10,2)",   true,  ["sum", "min", "max", "avg", "count"], displayFormat: "C2"),
            DataType.Create(BooleanId,  CustomFieldDataType.Boolean,  "System.Boolean",         "boolean",         true,  ["count"]),
            DataType.Create(DateId,     CustomFieldDataType.Date,     "System.DateOnly",        "date",            false, []),
            DataType.Create(DateTimeId, CustomFieldDataType.DateTime, "System.DateTimeOffset",  "timestamptz",     false, []),
            DataType.Create(JsonId,     CustomFieldDataType.Json,     "System.Text.Json.JsonElement", "jsonb",     false, [])
        );
    }
}
