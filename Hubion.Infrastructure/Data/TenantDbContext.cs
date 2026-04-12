using Hubion.Domain.Entities;
using Hubion.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Hubion.Infrastructure.Data;

/// <summary>
/// DbContext for all per-tenant tables (call_records, call_interactions, etc.).
/// Does NOT set a default schema — table names are unqualified.
/// The correct tenant schema is applied via PostgreSQL search_path on the connection,
/// set by TenantDbContextFactory for each request. See ARCHITECTURE.md §6.
/// </summary>
public class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public DbSet<CallRecord> CallRecords => Set<CallRecord>();
    public DbSet<CallInteraction> CallInteractions => Set<CallInteraction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CallRecordConfiguration());
        modelBuilder.ApplyConfiguration(new CallInteractionConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Auto-update updated_at on any modified CallRecord
        foreach (var entry in ChangeTracker.Entries<CallRecord>())
        {
            if (entry.State == EntityState.Modified)
                entry.Property("UpdatedAt").CurrentValue = DateTimeOffset.UtcNow;
        }
        return base.SaveChangesAsync(ct);
    }
}
