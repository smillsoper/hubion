using Hubion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hubion.Infrastructure.Data.Configurations;

public class FlowSessionConfiguration : IEntityTypeConfiguration<FlowSession>
{
    public void Configure(EntityTypeBuilder<FlowSession> builder)
    {
        builder.ToTable("flow_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(s => s.FlowId)
            .HasColumnName("flow_id")
            .IsRequired();

        builder.Property(s => s.FlowVersion)
            .HasColumnName("flow_version")
            .IsRequired();

        builder.Property(s => s.CallRecordId)
            .HasColumnName("call_record_id")
            .IsRequired();

        builder.Property(s => s.InteractionId)
            .HasColumnName("interaction_id")
            .IsRequired();

        builder.Property(s => s.AgentId)
            .HasColumnName("agent_id")
            .IsRequired();

        builder.Property(s => s.CurrentNodeId)
            .HasColumnName("current_node_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        // Variable store — all {{flow.*}}, {{input.*}}, {{api.*}} values for this session
        builder.Property(s => s.VariableStore)
            .HasColumnName("variable_store")
            .HasColumnType("jsonb")
            .IsRequired();

        // Ordered audit log of every node visited — for call trace view and flow relaunch
        builder.Property(s => s.ExecutionHistory)
            .HasColumnName("execution_history")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(s => s.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(s => s.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(s => new { s.CallRecordId, s.Status })
            .HasDatabaseName("idx_flow_sessions_call_record_status");

        builder.HasIndex(s => new { s.TenantId, s.AgentId, s.StartedAt })
            .HasDatabaseName("idx_flow_sessions_agent_date");

        // Active sessions by call record — most common lookup
        builder.HasIndex(s => s.CallRecordId)
            .HasFilter("status = 'active'")
            .HasDatabaseName("idx_flow_sessions_active_call");
    }
}
