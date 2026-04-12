using Microsoft.EntityFrameworkCore;

namespace Hubion.Infrastructure.Data;

public class HubionDbContext : DbContext
{
    public HubionDbContext(DbContextOptions<HubionDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        base.OnModelCreating(modelBuilder);
    }
}