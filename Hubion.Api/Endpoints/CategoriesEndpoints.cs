using Hubion.Application.Interfaces.Repositories;
using Hubion.Application.Services;
using Hubion.Domain.Entities;

namespace Hubion.Api.Endpoints;

public static class CategoriesEndpoints
{
    public static IEndpointRouteBuilder MapCategoriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories").RequireAuthorization();

        group.MapPost("", Create);
        group.MapGet("", GetRoots);
        group.MapGet("{id:guid}", GetById);

        // Product assignment
        var products = app.MapGroup("/api/v1/products").RequireAuthorization();
        products.MapPost("{id:guid}/categories/{categoryId:guid}", AssignProductToCategory);
        products.MapDelete("{id:guid}/categories/{categoryId:guid}", RemoveProductFromCategory);

        return app;
    }

    // ── POST /api/v1/categories ──────────────────────────────────────────────

    private static async Task<IResult> Create(
        CreateCategoryRequest req,
        IProductCategoryRepository categories,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var category = ProductCategory.Create(
            tenantContext.Current!.Id,
            req.Name,
            req.Slug,
            req.ParentId,
            req.DisplayOrder);

        await categories.AddAsync(category, ct);
        await categories.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/categories/{category.Id}", ToResponse(category));
    }

    // ── GET /api/v1/categories?parentId=x ───────────────────────────────────

    private static async Task<IResult> GetRoots(
        Guid? parentId,
        IProductCategoryRepository categories,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var list = parentId.HasValue
            ? await categories.GetChildrenAsync(parentId.Value, ct)
            : await categories.GetRootsAsync(ct);

        return Results.Ok(list.Select(ToResponse));
    }

    // ── GET /api/v1/categories/{id} ──────────────────────────────────────────

    private static async Task<IResult> GetById(
        Guid id,
        IProductCategoryRepository categories,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var category = await categories.GetByIdAsync(id, ct);
        return category is null ? Results.NotFound() : Results.Ok(ToResponse(category));
    }

    // ── POST /api/v1/products/{id}/categories/{categoryId} ──────────────────

    private static async Task<IResult> AssignProductToCategory(
        Guid id, Guid categoryId,
        IProductRepository products,
        IProductCategoryRepository categories,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var product = await products.GetByIdAsync(id, ct);
        if (product is null) return Results.NotFound(new { error = "Product not found." });

        var category = await categories.GetByIdAsync(categoryId, ct);
        if (category is null) return Results.NotFound(new { error = "Category not found." });

        product.AssignToCategory(category);
        await products.SaveChangesAsync(ct);

        return Results.Ok(ProductsEndpoints.ToResponse(product));
    }

    // ── DELETE /api/v1/products/{id}/categories/{categoryId} ────────────────

    private static async Task<IResult> RemoveProductFromCategory(
        Guid id, Guid categoryId,
        IProductRepository products,
        TenantContext tenantContext,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant) return Results.Unauthorized();

        var product = await products.GetByIdAsync(id, ct);
        if (product is null) return Results.NotFound(new { error = "Product not found." });

        product.RemoveFromCategory(categoryId);
        await products.SaveChangesAsync(ct);

        return Results.Ok(ProductsEndpoints.ToResponse(product));
    }

    // ── Response shape ───────────────────────────────────────────────────────

    internal static object ToResponse(ProductCategory c) => new
    {
        c.Id,
        c.TenantId,
        c.ParentId,
        c.Name,
        c.Slug,
        c.DisplayOrder,
        c.IsActive,
        Children = c.Children.Select(ToResponse),
        c.CreatedAt,
        c.UpdatedAt
    };
}

// ── Request records ──────────────────────────────────────────────────────────

public record CreateCategoryRequest(
    string Name,
    string Slug,
    Guid? ParentId = null,
    int DisplayOrder = 0);
