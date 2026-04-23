using Hubion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hubion.Infrastructure.Data.Configurations;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("product_attributes");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.TenantId).HasColumnName("tenant_id");
        builder.Property(a => a.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(a => a.Slug).HasColumnName("slug").HasMaxLength(200).IsRequired();
        builder.Property(a => a.DisplayOrder).HasColumnName("display_order");
        builder.Property(a => a.IsActive).HasColumnName("is_active");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        // Values — cascade: removing an attribute removes all its values
        builder.HasMany(a => a.Values)
            .WithOne()
            .HasForeignKey(v => v.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.TenantId).HasDatabaseName("ix_product_attributes_tenant_id");
        builder.HasIndex(a => new { a.TenantId, a.Slug })
            .IsUnique()
            .HasDatabaseName("ix_product_attributes_tenant_slug");
    }
}
