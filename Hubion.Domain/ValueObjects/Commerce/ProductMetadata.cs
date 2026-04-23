namespace Hubion.Domain.ValueObjects.Commerce;

/// <summary>
/// Opaque key-value metadata tag on a product.
/// Used for tenant-defined categorization, integration codes, and reporting labels.
/// </summary>
public record ProductFlag(string Name, string Value);

/// <summary>
/// An available autoship interval for a product.
/// If only one interval is defined it is auto-selected; otherwise the agent chooses.
/// </summary>
public record AutoShipInterval(int IntervalDays, string? AutoShipId = null);
