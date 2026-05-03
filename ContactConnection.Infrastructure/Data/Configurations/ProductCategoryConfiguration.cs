using ContactConnection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactConnection.Infrastructure.Data.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("product_categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.TenantId).HasColumnName("tenant_id");
        builder.Property(c => c.ParentId).HasColumnName("parent_id");
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Slug).HasColumnName("slug").HasMaxLength(200).IsRequired();
        builder.Property(c => c.DisplayOrder).HasColumnName("display_order");
        builder.Property(c => c.IsActive).HasColumnName("is_active");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        // Self-referential for hierarchy — parent has many children; restrict delete (no cascade)
        builder.HasMany(c => c.Children)
            .WithOne()
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.TenantId).HasDatabaseName("ix_product_categories_tenant_id");
        builder.HasIndex(c => c.ParentId).HasDatabaseName("ix_product_categories_parent_id");
        builder.HasIndex(c => new { c.TenantId, c.Slug })
            .IsUnique()
            .HasDatabaseName("ix_product_categories_tenant_slug");
    }
}
