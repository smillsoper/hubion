namespace ContactConnection.Domain.Entities;

/// <summary>
/// A discrete allowed value for a ProductAttribute (e.g. "Red" for attribute "Color").
/// Created only through ProductAttribute.AddValue() to maintain consistency.
/// </summary>
public class ProductAttributeValue
{
    public Guid Id { get; private set; }
    public Guid AttributeId { get; private set; }
    public string Value { get; private set; } = "";
    public int DisplayOrder { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Required by EF Core
    private ProductAttributeValue() { }

    public static ProductAttributeValue Create(Guid attributeId, string value, int displayOrder = 0)
        => new()
        {
            Id           = Guid.NewGuid(),
            AttributeId  = attributeId,
            Value        = value,
            DisplayOrder = displayOrder,
            CreatedAt    = DateTimeOffset.UtcNow
        };
}
