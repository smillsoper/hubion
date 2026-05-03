using System.Text.Json;
using ContactConnection.Domain.Entities;
using ContactConnection.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactConnection.Infrastructure.Data.Configurations;

public class CallInteractionConfiguration : IEntityTypeConfiguration<CallInteraction>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<CallInteraction> builder)
    {
        builder.ToTable("call_interactions");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.CallRecordId).HasColumnName("call_record_id");
        builder.Property(i => i.InteractionNumber).HasColumnName("interaction_number");

        builder.Property(i => i.Type)
            .HasColumnName("type").IsRequired().HasMaxLength(50);
        builder.Property(i => i.FlowId).HasColumnName("flow_id");
        builder.Property(i => i.FlowVersion).HasColumnName("flow_version");
        builder.Property(i => i.Disposition)
            .HasColumnName("disposition").HasMaxLength(50);
        builder.Property(i => i.Status)
            .HasColumnName("status").IsRequired().HasMaxLength(20);
        builder.Property(i => i.CartId).HasColumnName("cart_id");
        builder.Property(i => i.StartedAt).HasColumnName("started_at");
        builder.Property(i => i.CompletedAt).HasColumnName("completed_at");

        builder.Property(i => i.FlowExecutionState)
            .HasColumnName("flow_execution_state")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb");

        builder.Property(i => i.CommitmentEvents)
            .HasColumnName("commitment_events")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<CommitmentEvent>>(v, JsonOptions) ?? new())
            .HasDefaultValueSql("'[]'::jsonb");

        builder.Property(i => i.CustomFields)
            .HasColumnName("custom_fields")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb");

        builder.HasIndex(i => i.CallRecordId)
            .HasDatabaseName("idx_call_interactions_call_record");
    }
}
