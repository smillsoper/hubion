namespace ContactConnection.Domain.ValueObjects.Commerce;

/// <summary>
/// A shipping method available for a product, with delivery window and optional surcharge.
/// When Product.ShipMethodPerItem is true, the agent selects from this list per cart item.
/// </summary>
public record ProductShipMethod(
    string MethodCode,
    string Description,
    int MinDeliveryDays,
    int MaxDeliveryDays,
    bool BusinessDaysOnly,
    decimal Surcharge);
