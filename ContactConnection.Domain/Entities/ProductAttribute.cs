namespace ContactConnection.Domain.Entities;

/// <summary>
/// Attribute definition (e.g. "Color", "Size", "Material") for a tenant's product catalog.
/// Attribute values are owned by this entity and created via AddValue().
/// </summary>
public class ProductAttribute
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<ProductAttributeValue> _values = [];
    public IReadOnlyList<ProductAttributeValue> Values => _values.AsReadOnly();

    // Required by EF Core
    private ProductAttribute() { }

    public static ProductAttribute Create(
        Guid tenantId,
        string name,
        string slug,
        int displayOrder = 0)
    {
        var now = DateTimeOffset.UtcNow;
        return new ProductAttribute
        {
            Id           = Guid.NewGuid(),
            TenantId     = tenantId,
            Name         = name,
            Slug         = slug,
            DisplayOrder = displayOrder,
            IsActive     = true,
            CreatedAt    = now,
            UpdatedAt    = now
        };
    }

    public ProductAttributeValue AddValue(string value, int displayOrder = 0)
    {
        var v = ProductAttributeValue.Create(Id, value, displayOrder);
        _values.Add(v);
        return v;
    }
}
