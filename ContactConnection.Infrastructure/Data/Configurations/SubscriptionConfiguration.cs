using ContactConnection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactConnection.Infrastructure.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(s => s.Id);

        // Identity
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.TenantId).HasColumnName("tenant_id");

        // Source references
        builder.Property(s => s.CallRecordId).HasColumnName("call_record_id").IsRequired(false);
        builder.Property(s => s.OriginalOrderId).HasColumnName("original_order_id");
        builder.Property(s => s.OriginalOrderLineId).HasColumnName("original_order_line_id");

        // Product/offer snapshot
        builder.Property(s => s.OfferId).HasColumnName("offer_id");
        builder.Property(s => s.ProductId).HasColumnName("product_id");
        builder.Property(s => s.Sku).HasColumnName("sku").HasMaxLength(100).IsRequired();
        builder.Property(s => s.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(s => s.Quantity).HasColumnName("quantity");
        builder.Property(s => s.UnitPrice).HasColumnName("unit_price").HasColumnType("numeric(18,2)");
        builder.Property(s => s.Shipping).HasColumnName("shipping").HasColumnType("numeric(18,2)");

        // Schedule
        builder.Property(s => s.IntervalDays).HasColumnName("interval_days");
        builder.Property(s => s.NextShipDate).HasColumnName("next_ship_date");
        builder.Property(s => s.LastShipDate).HasColumnName("last_ship_date");
        builder.Property(s => s.ShipmentCount).HasColumnName("shipment_count");

        // Lifecycle
        builder.Property(s => s.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(s => s.CancelledAt).HasColumnName("cancelled_at");

        // Audit
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(s => s.TenantId).HasDatabaseName("ix_subscriptions_tenant_id");
        builder.HasIndex(s => s.CallRecordId).HasDatabaseName("ix_subscriptions_call_record_id");
        builder.HasIndex(s => new { s.Status, s.NextShipDate }).HasDatabaseName("ix_subscriptions_due");
    }
}
