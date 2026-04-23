using System.Text.Json;
using Hubion.Domain.Entities;
using Hubion.Domain.ValueObjects.Commerce;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hubion.Infrastructure.Data.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines");
        builder.HasKey(l => l.Id);

        // Identity
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.OrderId).HasColumnName("order_id");
        builder.Property(l => l.TenantId).HasColumnName("tenant_id");

        // Offer/product snapshot
        builder.Property(l => l.OfferId).HasColumnName("offer_id");
        builder.Property(l => l.ProductId).HasColumnName("product_id");
        builder.Property(l => l.Sku).HasColumnName("sku").HasMaxLength(100).IsRequired();
        builder.Property(l => l.Description).HasColumnName("description").HasMaxLength(500).IsRequired();

        // Quantity and pricing
        builder.Property(l => l.Quantity).HasColumnName("quantity");
        builder.Property(l => l.UnitPrice).HasColumnName("unit_price").HasColumnType("numeric(18,2)");
        builder.Property(l => l.ExtendedPrice).HasColumnName("extended_price").HasColumnType("numeric(18,2)");
        builder.Property(l => l.Shipping).HasColumnName("shipping").HasColumnType("numeric(18,2)");
        builder.Property(l => l.SalesTax).HasColumnName("sales_tax").HasColumnType("numeric(18,2)");
        builder.Property(l => l.Weight).HasColumnName("weight").HasColumnType("numeric(10,4)");

        // Tax / shipping flags
        builder.Property(l => l.ShippingExempt).HasColumnName("shipping_exempt");
        builder.Property(l => l.TaxExempt).HasColumnName("tax_exempt");
        builder.Property(l => l.OnBackOrder).HasColumnName("on_back_order");

        // AutoShip
        builder.Property(l => l.AutoShip).HasColumnName("auto_ship");
        builder.Property(l => l.AutoShipIntervalDays).HasColumnName("auto_ship_interval_days");

        // Sales metadata
        builder.Property(l => l.IsUpsell).HasColumnName("is_upsell");
        builder.Property(l => l.MixMatchCode).HasColumnName("mix_match_code").HasMaxLength(100);

        // Shipping detail
        builder.Property(l => l.ShipMethod).HasColumnName("ship_method").HasMaxLength(100);
        builder.Property(l => l.DeliveryMessage).HasColumnName("delivery_message").HasMaxLength(500);
        builder.Property(l => l.ShipToJson).HasColumnName("ship_to").HasColumnType("jsonb");

        // Geographic surcharges
        builder.Property(l => l.CanadaSurcharge).HasColumnName("canada_surcharge").HasColumnType("numeric(18,2)");
        builder.Property(l => l.AKHISurcharge).HasColumnName("akhi_surcharge").HasColumnType("numeric(18,2)");
        builder.Property(l => l.OutlyingUSSurcharge).HasColumnName("outlying_us_surcharge").HasColumnType("numeric(18,2)");
        builder.Property(l => l.ForeignSurcharge).HasColumnName("foreign_surcharge").HasColumnType("numeric(18,2)");

        // JSONB — complex snapshots
        builder.Property(l => l.Payments)
            .HasColumnName("payments").HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<PaymentInstallment>>(v, JsonOptions) ?? new())
            .HasDefaultValueSql("'[]'::jsonb");

        builder.Property(l => l.PersonalizationAnswers)
            .HasColumnName("personalization_answers").HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<CartPersonalizationAnswer>>(v, JsonOptions) ?? new())
            .HasDefaultValueSql("'[]'::jsonb");

        builder.Property(l => l.KitSelections)
            .HasColumnName("kit_selections").HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<CartKitSelection>>(v, JsonOptions) ?? new())
            .HasDefaultValueSql("'[]'::jsonb");

        // Fulfillment
        builder.Property(l => l.FulfillmentStatus).HasColumnName("fulfillment_status").HasMaxLength(50).IsRequired();
        builder.Property(l => l.TrackingNumber).HasColumnName("tracking_number").HasMaxLength(200);
        builder.Property(l => l.ShippedAt).HasColumnName("shipped_at");
        builder.Property(l => l.DeliveredAt).HasColumnName("delivered_at");
        builder.Property(l => l.CancelledAt).HasColumnName("cancelled_at");

        // Audit
        builder.Property(l => l.CreatedAt).HasColumnName("created_at");
        builder.Property(l => l.UpdatedAt).HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(l => l.OrderId).HasDatabaseName("ix_order_lines_order_id");
        builder.HasIndex(l => l.TenantId).HasDatabaseName("ix_order_lines_tenant_id");

        // FK to Order is configured from OrderConfiguration (cascade delete)
    }
}
