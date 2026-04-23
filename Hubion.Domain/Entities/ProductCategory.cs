namespace Hubion.Domain.Entities;

/// <summary>
/// Hierarchical category node for organizing products.
/// Root categories have null ParentId. Children are loaded on demand.
/// </summary>
public class ProductCategory
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid? ParentId { get; private set; }
    public string Name { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation — immediate children only (not recursive)
    private readonly List<ProductCategory> _children = [];
    public IReadOnlyList<ProductCategory> Children => _children.AsReadOnly();

    // Required by EF Core
    private ProductCategory() { }

    public static ProductCategory Create(
        Guid tenantId,
        string name,
        string slug,
        Guid? parentId = null,
        int displayOrder = 0)
    {
        var now = DateTimeOffset.UtcNow;
        return new ProductCategory
        {
            Id           = Guid.NewGuid(),
            TenantId     = tenantId,
            ParentId     = parentId,
            Name         = name,
            Slug         = slug,
            DisplayOrder = displayOrder,
            IsActive     = true,
            CreatedAt    = now,
            UpdatedAt    = now
        };
    }

    public void Rename(string name, string slug)
    {
        Name      = name;
        Slug      = slug;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Activate()   { IsActive = true;  UpdatedAt = DateTimeOffset.UtcNow; }
}
