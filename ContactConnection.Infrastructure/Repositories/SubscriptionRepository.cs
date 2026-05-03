using ContactConnection.Application.Interfaces.Repositories;
using ContactConnection.Domain.Entities;
using ContactConnection.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactConnection.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private TenantDbContext? _ctx;
    private readonly ScopedTenantDbContextFactory _factory;

    public SubscriptionRepository(ScopedTenantDbContextFactory factory)
        => _factory = factory;

    private TenantDbContext Ctx => _ctx ??= _factory.Create();

    public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Ctx.Subscriptions.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<List<Subscription>> GetByCallRecordIdAsync(Guid callRecordId, CancellationToken ct = default)
        => Ctx.Subscriptions
            .Where(s => s.CallRecordId == callRecordId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

    public Task<List<Subscription>> GetDueAsync(DateTimeOffset cutoff, CancellationToken ct = default)
        => Ctx.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active && s.NextShipDate <= cutoff)
            .OrderBy(s => s.NextShipDate)
            .ToListAsync(ct);

    public async Task AddAsync(Subscription subscription, CancellationToken ct = default)
        => await Ctx.Subscriptions.AddAsync(subscription, ct);

    public async Task AddRangeAsync(IEnumerable<Subscription> subscriptions, CancellationToken ct = default)
        => await Ctx.Subscriptions.AddRangeAsync(subscriptions, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Ctx.SaveChangesAsync(ct);
}
