using Hubion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hubion.Infrastructure.Data.Configurations;

public class CustomFieldValueConfiguration : IEntityTypeConfiguration<CustomFieldValue>
{
    public void Configure(EntityTypeBuilder<CustomFieldValue> builder)
    {
        builder.ToTable("custom_field_values");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.CallRecordId).HasColumnName("call_record_id").IsRequired();
        builder.Property(v => v.DefinitionId).HasColumnName("definition_id").IsRequired();

        builder.Property(v => v.ValueString).HasColumnName("value_string");
        builder.Property(v => v.ValueInteger).HasColumnName("value_integer");
        builder.Property(v => v.ValueDecimal).HasColumnName("value_decimal").HasColumnType("numeric(18,6)");
        builder.Property(v => v.ValueBoolean).HasColumnName("value_boolean");
        builder.Property(v => v.ValueDate).HasColumnName("value_date");
        builder.Property(v => v.ValueDatetime).HasColumnName("value_datetime");
        builder.Property(v => v.ValueJson).HasColumnName("value_json").HasColumnType("jsonb");
        builder.Property(v => v.StoredAt).HasColumnName("stored_at");

        builder.HasOne(v => v.Definition)
            .WithMany()
            .HasForeignKey(v => v.DefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.CallRecordId, v.DefinitionId })
            .IsUnique()
            .HasDatabaseName("ix_cfv_call_record_definition_unique");

        builder.HasIndex(v => v.CallRecordId).HasDatabaseName("ix_cfv_call_record_id");
    }
}
