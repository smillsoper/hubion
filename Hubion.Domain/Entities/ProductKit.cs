namespace Hubion.Domain.Entities;

/// <summary>
/// A kit component attached to a parent product.
///
/// Fixed kits: the child product is always included at the given Qty when the parent is ordered.
/// Variable kits: the agent selects from ChoiceSkus at order time (Qty units, MultiSelect or single).
///
/// Kits are one level deep — CRMPro supported recursive kits but in practice they were never used.
/// </summary>
public class ProductKit
{
    public Guid Id { get; private set; }
    public Guid ParentProductId { get; private set; }

    // Fixed kit: ChildProductId is set. Variable kit: ChildProductId is null, ChoiceSkus has options.
    public Guid? ChildProductId { get; private set; }
    public int Qty { get; private set; }

    // Variable kit fields (only meaningful when IsVariable = true)
    public bool IsVariable { get; private set; }
    public string? KitPrompt { get; private set; }
    public bool MultiSelect { get; private set; }
    public List<string> ChoiceSkus { get; private set; } = [];

    // Navigation
    public Product Parent { get; private set; } = null!;
    public Product? Child { get; private set; }

    // Required by EF Core
    private ProductKit() { }

    /// <summary>Fixed kit — parent always ships with ChildProductId at Qty.</summary>
    public static ProductKit CreateFixed(Guid parentProductId, Guid childProductId, int qty)
        => new()
        {
            Id              = Guid.NewGuid(),
            ParentProductId = parentProductId,
            ChildProductId  = childProductId,
            Qty             = qty,
            IsVariable      = false
        };

    /// <summary>Variable kit — agent selects from ChoiceSkus; Qty units, single or multi-select.</summary>
    public static ProductKit CreateVariable(
        Guid parentProductId, string kitPrompt, int qty, bool multiSelect, List<string> choiceSkus)
        => new()
        {
            Id              = Guid.NewGuid(),
            ParentProductId = parentProductId,
            Qty             = qty,
            IsVariable      = true,
            KitPrompt       = kitPrompt,
            MultiSelect     = multiSelect,
            ChoiceSkus      = choiceSkus
        };
}
