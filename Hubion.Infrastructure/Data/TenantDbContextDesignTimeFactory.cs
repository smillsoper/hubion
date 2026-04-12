using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hubion.Infrastructure.Data;

/// <summary>
/// Used only by EF Core tooling (dotnet ef migrations add/update).
/// Points at tenant_tms schema for migration generation — schema name doesn't matter
/// for the migration content since all table names are unqualified.
/// At runtime, TenantDbContextFactory applies the correct search_path per tenant.
/// </summary>
public class TenantDbContextDesignTimeFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        const string connStr =
            "Host=localhost;Port=5432;Database=hubion_master;" +
            "Username=hubion;Password=hubion_dev;" +
            "Search Path=tenant_tms,public";

        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseNpgsql(connStr, opts => opts.MigrationsAssembly("Hubion.Infrastructure"))
            .Options;

        return new TenantDbContext(options);
    }
}
