namespace ContactConnection.Domain.ValueObjects;

/// <summary>
/// JSONB envelope for the addresses field on call_records.
/// Both addresses are optional — a service-only call may have neither.
/// </summary>
public class CallAddresses
{
    public AddressData? Billing { get; set; }
    public AddressData? Shipping { get; set; }
}
