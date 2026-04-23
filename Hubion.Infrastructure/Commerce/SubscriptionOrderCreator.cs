using Hubion.Application.Interfaces.Repositories;
using Hubion.Application.Interfaces.Services;
using Hubion.Domain.Entities;
using Hubion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hubion.Infrastructure.Commerce;

/// <summary>
/// Creates a renewal Order + OrderLine for a due subscription.
/// Confirms inventory (via Product.Confirm) before writing the order.
/// Throws InvalidOperationException if the product is NoBackorder with insufficient stock.
/// </summary>
public class SubscriptionOrderCreator : ISubscriptionOrderCreator
{
    private readonly IOrderRepository _orders;
    private readonly ScopedTenantDbContextFactory _factory;

    public SubscriptionOrderCreator(IOrderRepository orders, ScopedTenantDbContextFactory factory)
    {
        _orders  = orders;
        _factory = factory;
    }

    public async Task<Order> CreateRenewalOrderAsync(Subscription subscription, CancellationToken ct = default)
    {
        var ctx = _factory.Create();

        // Load the product to confirm inventory
        var product = await ctx.Products.FirstOrDefaultAsync(p => p.Id == subscription.ProductId, ct)
            ?? throw new InvalidOperationException(
                $"Product {subscription.ProductId} not found for subscription {subscription.Id}.");

        // Validate stock before committing
        if (!product.CanAddToCart(subscription.Quantity))
            throw new InvalidOperationException(
                $"Insufficient stock for subscription {subscription.Id} (SKU: {subscription.Sku}). " +
                $"QtyAvailable={product.QtyAvailable}, QtyReserved={product.QtyReserved}, Requested={subscription.Quantity}.");

        // Confirm inventory — direct decrement, no reservation cycle for autoship
        product.Confirm(subscription.Quantity);
        await ctx.SaveChangesAsync(ct);

        // Build order + line
        var orderId = Guid.NewGuid();
        var salesTax = 0m;   // tax recalculation for autoship is a future enhancement
        var total    = (subscription.UnitPrice * subscription.Quantity) + subscription.Shipping + salesTax;

        var line = new List<OrderLine>
        {
            OrderLine.CreateFromSubscription(orderId, subscription.TenantId, subscription)
        };

        var order = Order.CreateFromSubscription(
            id:        orderId,
            tenantId:  subscription.TenantId,
            subtotal:  subscription.UnitPrice * subscription.Quantity,
            shipping:  subscription.Shipping,
            salesTax:  salesTax,
            total:     total,
            lines:     line);

        await _orders.AddAsync(order, ct);
        await _orders.SaveChangesAsync(ct);

        return order;
    }
}
