using ContactConnection.Application.Interfaces.Repositories;
using ContactConnection.Application.Services;
using ContactConnection.Domain.Entities;
using ContactConnection.Domain.ValueObjects.Commerce;

namespace ContactConnection.Api.Endpoints;

public static class OffersEndpoints
{
    public static IEndpointRouteBuilder MapOffersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/offers").RequireAuthorization();

        group.MapPost("", Create);
        group.MapGet("", GetActive);
        group.MapGet("{id:guid}", GetById);
        group.MapPost("{id:guid}/activate", Activate);
        group.MapPost("{id:guid}/deactivate", Deactivate);
        group.MapGet("product/{productId:guid}", GetByProduct);

        return app;
    }

    // ── POST /api/v1/offers ──────────────────────────────────────────────────

    private static async Task<IResult> Create(
        CreateOfferRequest req,
        IOfferRepository offers,
        IProductRepository products,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Results.Unauthorized();

        var product = await products.GetByIdAsync(req.ProductId, ct);
        if (product is null)
            return Results.NotFound(new { error = $"Product {req.ProductId} not found." });

        var offer = Offer.Create(
            tenantContext.Current!.Id,
            req.ProductId,
            req.Name,
            req.FullPrice,
            req.Shipping);

        if (req.Payments is { Count: > 0 })
            offer.SetPricing(
                req.FullPrice,
                req.Payments,
                req.QuantityPriceBreaks,
                req.MixMatchPriceBreaks,
                req.AllowPriceOverride);

        if (req.MixMatchCode is not null)
            offer.SetMixMatch(req.MixMatchCode, req.MixMatchPriceBreaks);

        if (req.IsUpsell)
            offer.SetUpsell(
                true,
                req.UpsellQty,
                req.UpsellQtyOfEntry,
                req.UpsellCommission,
                req.UpsellClientAmount);

        if (req.AutoShip)
            offer.SetAutoShip(true, req.AutoShipOptional, req.AutoShipIntervals ?? []);

        if (req.Personalization is { Count: > 0 })
            offer.SetPersonalization(req.Personalization);

        if (req.ValidFrom.HasValue || req.ValidTo.HasValue)
            offer.SetCampaignWindow(req.ValidFrom, req.ValidTo);

        offer.SetShipping(
            req.Shipping,
            req.ShippingExempt,
            req.TaxExempt,
            req.ShipMethodPerItem,
            req.AllowShipTo,
            req.ShipToRequired,
            req.AllowDeliveryMessage,
            req.ShipMethods);

        await offers.AddAsync(offer, ct);
        await offers.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/offers/{offer.Id}", ToResponse(offer));
    }

    // ── GET /api/v1/offers ───────────────────────────────────────────────────

    private static async Task<IResult> GetActive(
        IOfferRepository offers,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Results.Unauthorized();

        var list = await offers.GetActiveAsync(ct);
        return Results.Ok(list.Select(ToResponse));
    }

    // ── GET /api/v1/offers/{id} ──────────────────────────────────────────────

    private static async Task<IResult> GetById(
        Guid id,
        IOfferRepository offers,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Results.Unauthorized();

        var offer = await offers.GetByIdAsync(id, ct);
        return offer is null ? Results.NotFound() : Results.Ok(ToResponse(offer));
    }

    // ── POST /api/v1/offers/{id}/activate ────────────────────────────────────

    private static async Task<IResult> Activate(
        Guid id,
        IOfferRepository offers,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Results.Unauthorized();

        var offer = await offers.GetByIdAsync(id, ct);
        if (offer is null) return Results.NotFound();

        offer.Activate();
        await offers.SaveChangesAsync(ct);
        return Results.Ok(ToResponse(offer));
    }

    // ── POST /api/v1/offers/{id}/deactivate ──────────────────────────────────

    private static async Task<IResult> Deactivate(
        Guid id,
        IOfferRepository offers,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Results.Unauthorized();

        var offer = await offers.GetByIdAsync(id, ct);
        if (offer is null) return Results.NotFound();

        offer.Deactivate();
        await offers.SaveChangesAsync(ct);
        return Results.Ok(ToResponse(offer));
    }

    // ── GET /api/v1/offers/product/{productId} ───────────────────────────────

    private static async Task<IResult> GetByProduct(
        Guid productId,
        IOfferRepository offers,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Results.Unauthorized();

        var list = await offers.GetByProductIdAsync(productId, ct);
        return Results.Ok(list.Select(ToResponse));
    }

    // ── Response shape ───────────────────────────────────────────────────────

    internal static object ToResponse(Offer o) => new
    {
        o.Id,
        o.ProductId,
        o.Name,
        o.FullPrice,
        o.Shipping,
        o.TaxExempt,
        o.ShippingExempt,
        o.AllowPriceOverride,
        o.IsActive,
        o.ValidFrom,
        o.ValidTo,
        o.MixMatchCode,
        Upsell = new
        {
            o.IsUpsell,
            o.UpsellQty,
            o.UpsellQtyOfEntry,
            o.UpsellCommission,
            o.UpsellClientAmount
        },
        AutoShip = new
        {
            o.AutoShip,
            o.AutoShipOptional,
            o.AutoShipIntervals
        },
        ShipOptions = new
        {
            o.ShipMethodPerItem,
            o.AllowShipTo,
            o.ShipToRequired,
            o.AllowDeliveryMessage,
            o.ShipMethods
        },
        Pricing = new
        {
            o.Payments,
            o.QuantityPriceBreaks,
            o.MixMatchPriceBreaks
        },
        o.Personalization,
        o.Flags,
        o.CreatedAt,
        o.UpdatedAt
    };
}

// ── Request records ──────────────────────────────────────────────────────────

public record CreateOfferRequest(
    Guid ProductId,
    string Name,
    decimal FullPrice,
    decimal Shipping = 0,
    bool TaxExempt = false,
    bool ShippingExempt = false,
    bool AllowPriceOverride = false,
    string? MixMatchCode = null,
    bool IsUpsell = false,
    int UpsellQty = 0,
    int UpsellQtyOfEntry = 0,
    decimal UpsellCommission = 0,
    decimal UpsellClientAmount = 0,
    bool AutoShip = false,
    bool AutoShipOptional = false,
    bool ShipMethodPerItem = false,
    bool AllowShipTo = false,
    bool ShipToRequired = false,
    bool AllowDeliveryMessage = false,
    DateTimeOffset? ValidFrom = null,
    DateTimeOffset? ValidTo = null,
    List<PaymentInstallment>? Payments = null,
    List<QuantityPriceBreak>? QuantityPriceBreaks = null,
    List<QuantityPriceBreak>? MixMatchPriceBreaks = null,
    List<AutoShipInterval>? AutoShipIntervals = null,
    List<ProductShipMethod>? ShipMethods = null,
    List<PersonalizationPrompt>? Personalization = null);
