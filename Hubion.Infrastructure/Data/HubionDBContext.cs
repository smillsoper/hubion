using Hubion.Domain.Entities;
using Hubion.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Hubion.Infrastructure.Data;

public class HubionDbContext : DbContext
{
    public HubionDbContext(DbContextOptions<HubionDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}