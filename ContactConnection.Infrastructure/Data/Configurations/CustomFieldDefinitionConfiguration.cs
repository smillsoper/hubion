using ContactConnection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactConnection.Infrastructure.Data.Configurations;

public class CustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CustomFieldDefinition>
{
    public void Configure(EntityTypeBuilder<CustomFieldDefinition> builder)
    {
        builder.ToTable("custom_field_definitions");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(d => d.ClientId).HasColumnName("client_id");
        builder.Property(d => d.CampaignId).HasColumnName("campaign_id");
        builder.Property(d => d.FieldName).HasColumnName("field_name").HasMaxLength(100).IsRequired();
        builder.Property(d => d.DisplayLabel).HasColumnName("display_label").HasMaxLength(200).IsRequired();
        builder.Property(d => d.DataTypeName).HasColumnName("data_type_name").HasMaxLength(50).IsRequired();
        builder.Property(d => d.IsRequired).HasColumnName("is_required");
        builder.Property(d => d.ValidationRules).HasColumnName("validation_rules").HasColumnType("jsonb");
        builder.Property(d => d.DisplayOrder).HasColumnName("display_order");
        builder.Property(d => d.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        // Ignore computed property — not stored
        builder.Ignore(d => d.ScopeRank);

        builder.HasIndex(d => d.TenantId).HasDatabaseName("ix_cfd_tenant_id");
        builder.HasIndex(d => new { d.TenantId, d.ClientId, d.CampaignId })
            .HasDatabaseName("ix_cfd_scope");
        builder.HasIndex(d => new { d.TenantId, d.FieldName, d.ClientId, d.CampaignId })
            .IsUnique()
            .HasDatabaseName("ix_cfd_tenant_field_scope_unique");
    }
}
