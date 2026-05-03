namespace ContactConnection.Domain.Entities;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Subdomain { get; private set; } = string.Empty;
    public string? CustomDomain { get; private set; }
    public string SchemaName { get; private set; } = string.Empty;
    public string PlanTier { get; private set; } = string.Empty;
    public string Timezone { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset? TrialExpiresAt { get; private set; }
    public string? BillingContact { get; private set; }
    public TenantFeatureFlags FeatureFlags { get; private set; } = TenantFeatureFlags.Default();
    public DateTimeOffset CreatedAt { get; private set; }

    // Required by EF Core
    private Tenant() { }

    public static Tenant Create(string name, string subdomain, string planTier, string timezone)
    {
        var normalized = subdomain.ToLowerInvariant();
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Subdomain = normalized,
            SchemaName = $"tenant_{normalized}",
            PlanTier = planTier,
            Timezone = timezone,
            IsActive = true,
            FeatureFlags = TenantFeatureFlags.Default(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void SetCustomDomain(string? customDomain) => CustomDomain = customDomain;
    public void SetBillingContact(string? billingContact) => BillingContact = billingContact;
    public void SetTrialExpiry(DateTimeOffset? expiresAt) => TrialExpiresAt = expiresAt;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void UpdateFeatureFlags(TenantFeatureFlags flags) => FeatureFlags = flags;
}
