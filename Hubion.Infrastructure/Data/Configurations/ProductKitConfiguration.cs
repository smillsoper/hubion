using System.Text.Json;
using Hubion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hubion.Infrastructure.Data.Configurations;

public class ProductKitConfiguration : IEntityTypeConfiguration<ProductKit>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<ProductKit> builder)
    {
        builder.ToTable("product_kits");
        builder.HasKey(k => k.Id);

        builder.Property(k => k.Id).HasColumnName("id");
        builder.Property(k => k.ParentProductId).HasColumnName("parent_product_id");
        builder.Property(k => k.ChildProductId).HasColumnName("child_product_id");
        builder.Property(k => k.Qty).HasColumnName("qty");
        builder.Property(k => k.IsVariable).HasColumnName("is_variable");
        builder.Property(k => k.KitPrompt).HasColumnName("kit_prompt").HasMaxLength(500);
        builder.Property(k => k.MultiSelect).HasColumnName("multi_select");

        builder.Property(k => k.ChoiceSkus)
            .HasColumnName("choice_skus").HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new())
            .HasDefaultValueSql("'[]'::jsonb");

        builder.HasIndex(k => k.ParentProductId).HasDatabaseName("ix_product_kits_parent");
        builder.HasIndex(k => k.ChildProductId).HasDatabaseName("ix_product_kits_child");

        // The Parent navigation is configured from ProductConfiguration.
        // Child FK (optional — null for variable kits)
        builder.HasOne(k => k.Child)
            .WithMany()
            .HasForeignKey(k => k.ChildProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
