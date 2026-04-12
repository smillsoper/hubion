namespace Hubion.Domain.Entities;

public class TenantFeatureFlags
{
    public bool TelephonyNative { get; set; }
    public bool TelephonyByod { get; set; }
    public bool WebAutomation { get; set; }
    public bool OmsBuiltIn { get; set; }
    public bool ShopifyAdapter { get; set; }
    public bool AdvancedReporting { get; set; }
    public bool ParallelQueuing { get; set; }
    public bool ApiBuilder { get; set; }

    public static TenantFeatureFlags Default() => new()
    {
        TelephonyNative = false,
        TelephonyByod = false,
        WebAutomation = false,
        OmsBuiltIn = false,
        ShopifyAdapter = false,
        AdvancedReporting = false,
        ParallelQueuing = false,
        ApiBuilder = false
    };
}
