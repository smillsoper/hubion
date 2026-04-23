using Hubion.Application.Interfaces.Repositories;
using Hubion.Application.Interfaces.Services;
using Hubion.Application.Services;
using Hubion.Domain.Entities;

namespace Hubion.Api.Endpoints;

public static class SubscriptionsEndpoints
{
    public static IEndpointRouteBuilder MapSubscriptionsEndpoints(this IEndpointRouteBuilder app)
    {
        var subs = app.MapGroup("/api/v1/subscriptions").RequireAuthorization();

        subs.MapGet("{id:guid}", GetById);
        subs.MapPost("{id:guid}/pause", Pause);
        subs.MapPost("{id:guid}/resume", Resume);
        subs.MapPost("{id:guid}/cancel", Cancel);
        subs.MapPost("{id:guid}/process-now", ProcessNow);

        // List subscriptions for a call record
        var callRecords = app.MapGroup("/api/v1/call-records").RequireAuthorization();
        callRecords.MapGet("{callRecordId:guid}/subscriptions", GetByCallRecord);

        return app;
    }

    // ── GET /api/v1/subscriptions/{id} ───────────────────────────────────────

    private static async Task<IResult> GetById(
        Guid id,
        ISubscriptionRepository repo,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();
        var sub = await repo.GetByIdAsync(id, ct);
        return sub is null ? Results.NotFound() : Results.Ok(ToResponse(sub));
    }

    // ── GET /api/v1/call-records/{callRecordId}/subscriptions ────────────────

    private static async Task<IResult> GetByCallRecord(
        Guid callRecordId,
        ISubscriptionRepository repo,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();
        var subs = await repo.GetByCallRecordIdAsync(callRecordId, ct);
        return Results.Ok(subs.Select(ToResponse));
    }

    // ── POST /api/v1/subscriptions/{id}/pause ────────────────────────────────

    private static async Task<IResult> Pause(
        Guid id,
        ISubscriptionRepository repo,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();
        var sub = await repo.GetByIdAsync(id, ct);
        if (sub is null) return Results.NotFound();
        if (sub.Status == SubscriptionStatus.Cancelled)
            return Results.Conflict(new { Message = "Cannot pause a cancelled subscription." });
        sub.Pause();
        await repo.SaveChangesAsync(ct);
        return Results.Ok(ToResponse(sub));
    }

    // ── POST /api/v1/subscriptions/{id}/resume ───────────────────────────────

    private static async Task<IResult> Resume(
        Guid id,
        ISubscriptionRepository repo,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();
        var sub = await repo.GetByIdAsync(id, ct);
        if (sub is null) return Results.NotFound();
        if (sub.Status == SubscriptionStatus.Cancelled)
            return Results.Conflict(new { Message = "Cannot resume a cancelled subscription." });
        sub.Resume();
        await repo.SaveChangesAsync(ct);
        return Results.Ok(ToResponse(sub));
    }

    // ── POST /api/v1/subscriptions/{id}/cancel ───────────────────────────────

    private static async Task<IResult> Cancel(
        Guid id,
        ISubscriptionRepository repo,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();
        var sub = await repo.GetByIdAsync(id, ct);
        if (sub is null) return Results.NotFound();
        if (sub.Status == SubscriptionStatus.Cancelled)
            return Results.Ok(ToResponse(sub));   // idempotent
        sub.Cancel();
        await repo.SaveChangesAsync(ct);
        return Results.Ok(ToResponse(sub));
    }

    // ── POST /api/v1/subscriptions/{id}/process-now ──────────────────────────
    // Immediately triggers a renewal shipment regardless of NextShipDate.
    // Useful for manual retries, admin overrides, and integration testing.

    private static async Task<IResult> ProcessNow(
        Guid id,
        ISubscriptionRepository repo,
        ISubscriptionOrderCreator orderCreator,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var sub = await repo.GetByIdAsync(id, ct);
        if (sub is null) return Results.NotFound();

        if (sub.Status != SubscriptionStatus.Active)
            return Results.Conflict(new { Message = $"Subscription is {sub.Status} — only active subscriptions can be processed." });

        try
        {
            var order = await orderCreator.CreateRenewalOrderAsync(sub, ct);
            sub.RecordShipment();
            await repo.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                Subscription = ToResponse(sub),
                RenewalOrder = OrdersEndpoints.ToResponse(order)
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { Message = ex.Message });
        }
    }

    // ── Response mapping ─────────────────────────────────────────────────────

    public static object ToResponse(Subscription s) => new
    {
        s.Id,
        s.TenantId,
        s.CallRecordId,
        s.OriginalOrderId,
        s.OriginalOrderLineId,
        Product = new { s.OfferId, s.ProductId, s.Sku, s.Description },
        s.Quantity,
        s.UnitPrice,
        s.Shipping,
        Schedule = new
        {
            s.IntervalDays,
            s.NextShipDate,
            s.LastShipDate,
            s.ShipmentCount
        },
        s.Status,
        s.CancelledAt,
        s.CreatedAt,
        s.UpdatedAt
    };
}
