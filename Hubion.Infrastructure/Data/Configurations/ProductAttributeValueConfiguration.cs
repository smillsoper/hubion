using Hubion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hubion.Infrastructure.Data.Configurations;

public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.ToTable("product_attribute_values");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.AttributeId).HasColumnName("attribute_id");
        builder.Property(v => v.Value).HasColumnName("value").HasMaxLength(200).IsRequired();
        builder.Property(v => v.DisplayOrder).HasColumnName("display_order");
        builder.Property(v => v.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(v => v.AttributeId)
            .HasDatabaseName("ix_product_attribute_values_attribute_id");
    }
}
