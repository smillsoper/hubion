using Hubion.Application.Interfaces.Repositories;
using Hubion.Application.Services;
using Hubion.Domain.Entities;

namespace Hubion.Api.Endpoints;

public static class AttributesEndpoints
{
    public static IEndpointRouteBuilder MapAttributesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/attributes").RequireAuthorization();

        group.MapPost("", Create);
        group.MapGet("", GetAll);
        group.MapGet("{id:guid}", GetById);
        group.MapPost("{id:guid}/values", AddValue);

        // Product attribute-value assignment
        var products = app.MapGroup("/api/v1/products").RequireAuthorization();
        products.MapPost("{id:guid}/attribute-values/{valueId:guid}", AssignAttributeValue);
        products.MapDelete("{id:guid}/attribute-values/{valueId:guid}", RemoveAttributeValue);

        return app;
    }

    // ── POST /api/v1/attributes ──────────────────────────────────────────────

    private static async Task<IResult> Create(
        CreateAttributeRequest req,
        IProductAttributeRepository attributes,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var attribute = ProductAttribute.Create(
            tenantContext.Current!.Id,
            req.Name,
            req.Slug,
            req.DisplayOrder);

        await attributes.AddAsync(attribute, ct);
        await attributes.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/attributes/{attribute.Id}", ToResponse(attribute));
    }

    // ── GET /api/v1/attributes ───────────────────────────────────────────────

    private static async Task<IResult> GetAll(
        IProductAttributeRepository attributes,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var list = await attributes.GetAllAsync(ct);
        return Results.Ok(list.Select(ToResponse));
    }

    // ── GET /api/v1/attributes/{id} ──────────────────────────────────────────

    private static async Task<IResult> GetById(
        Guid id,
        IProductAttributeRepository attributes,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var attribute = await attributes.GetByIdAsync(id, ct);
        return attribute is null ? Results.NotFound() : Results.Ok(ToResponse(attribute));
    }

    // ── POST /api/v1/attributes/{id}/values ─────────────────────────────────

    private static async Task<IResult> AddValue(
        Guid id,
        AddAttributeValueRequest req,
        IProductAttributeRepository attributes,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var attribute = await attributes.GetByIdAsync(id, ct);
        if (attribute is null) return Results.NotFound();

        var value = ProductAttributeValue.Create(attribute.Id, req.Value, req.DisplayOrder);
        await attributes.AddValueAsync(value, ct);
        await attributes.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/attributes/{id}", ToValueResponse(value));
    }

    // ── POST /api/v1/products/{id}/attribute-values/{valueId} ───────────────

    private static async Task<IResult> AssignAttributeValue(
        Guid id, Guid valueId,
        IProductRepository products,
        IProductAttributeRepository attributes,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var product = await products.GetByIdAsync(id, ct);
        if (product is null) return Results.NotFound(new { error = "Product not found." });

        var value = await attributes.GetValueByIdAsync(valueId, ct);
        if (value is null) return Results.NotFound(new { error = "Attribute value not found." });

        product.SetAttributeValue(value);
        await products.SaveChangesAsync(ct);

        return Results.Ok(ProductsEndpoints.ToResponse(product));
    }

    // ── DELETE /api/v1/products/{id}/attribute-values/{valueId} ─────────────

    private static async Task<IResult> RemoveAttributeValue(
        Guid id, Guid valueId,
        IProductRepository products,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var product = await products.GetByIdAsync(id, ct);
        if (product is null) return Results.NotFound(new { error = "Product not found." });

        product.RemoveAttributeValue(valueId);
        await products.SaveChangesAsync(ct);

        return Results.Ok(ProductsEndpoints.ToResponse(product));
    }

    // ── Response shapes ──────────────────────────────────────────────────────

    internal static object ToResponse(ProductAttribute a) => new
    {
        a.Id,
        a.TenantId,
        a.Name,
        a.Slug,
        a.DisplayOrder,
        a.IsActive,
        Values = a.Values.Select(ToValueResponse),
        a.CreatedAt,
        a.UpdatedAt
    };

    internal static object ToValueResponse(ProductAttributeValue v) => new
    {
        v.Id,
        v.AttributeId,
        v.Value,
        v.DisplayOrder,
        v.CreatedAt
    };
}

// ── Request records ──────────────────────────────────────────────────────────

public record CreateAttributeRequest(
    string Name,
    string Slug,
    int DisplayOrder = 0);

public record AddAttributeValueRequest(
    string Value,
    int DisplayOrder = 0);
