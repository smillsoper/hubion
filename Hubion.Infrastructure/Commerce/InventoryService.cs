using Hubion.Application.Interfaces.Services;
using Hubion.Application.Services;
using Hubion.Domain.ValueObjects.Commerce;
using Hubion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hubion.Infrastructure.Commerce;

/// <summary>
/// Manages soft inventory reservations for cart operations.
///
/// All operations load products by the ProductId recorded in each CartItem,
/// apply the domain method, then save in a single SaveChangesAsync call.
///
/// Reserve is all-or-nothing: if any item cannot be reserved, no changes are committed
/// and the failing SKUs are returned to the caller.
/// </summary>
public class InventoryService : IInventoryService
{
    private TenantDbContext? _ctx;
    private readonly ScopedTenantDbContextFactory _factory;

    public InventoryService(ScopedTenantDbContextFactory factory)
        => _factory = factory;

    private TenantDbContext Ctx => _ctx ??= _factory.Create();

    public async Task<List<string>> ReserveCartAsync(CartDocument cart, CancellationToken ct = default)
    {
        if (cart.Items.Count == 0)
            return [];

        // Load all unique products referenced by the cart in one query.
        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var products   = await Ctx.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);

        var lookup = products.ToDictionary(p => p.Id);

        // Validate all items before making any changes (all-or-nothing).
        var failures = new List<string>();
        foreach (var item in cart.Items)
        {
            if (!lookup.TryGetValue(item.ProductId, out var product))
            {
                failures.Add(item.Sku);
                continue;
            }
            if (!product.CanAddToCart(item.Quantity))
                failures.Add(item.Sku);
        }

        if (failures.Count > 0)
            return failures;

        // All items are reservable — apply reservations.
        foreach (var item in cart.Items)
        {
            var product = lookup[item.ProductId];
            product.Reserve(item.Quantity);
        }

        await Ctx.SaveChangesAsync(ct);
        return [];
    }

    public async Task ReleaseCartAsync(CartDocument? cart, CancellationToken ct = default)
    {
        if (cart is null || cart.Items.Count == 0)
            return;

        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var products   = await Ctx.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);

        var lookup = products.ToDictionary(p => p.Id);

        foreach (var item in cart.Items)
        {
            if (lookup.TryGetValue(item.ProductId, out var product))
                product.Release(item.Quantity);
        }

        await Ctx.SaveChangesAsync(ct);
    }

    public async Task ConfirmCartAsync(CartDocument cart, CancellationToken ct = default)
    {
        if (cart.Items.Count == 0)
            return;

        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var products   = await Ctx.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);

        var lookup = products.ToDictionary(p => p.Id);

        foreach (var item in cart.Items)
        {
            if (lookup.TryGetValue(item.ProductId, out var product))
                product.Confirm(item.Quantity);
        }

        await Ctx.SaveChangesAsync(ct);
    }
}
