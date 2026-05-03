namespace ContactConnection.Domain.ValueObjects.Commerce;

/// <summary>
/// The data type an agent enters for a personalization prompt.
/// Simplified from CRMPro's clsFieldType designer component.
/// </summary>
public enum PersonalizationPromptType
{
    Text    = 0,
    Number  = 1,
    Date    = 2,
    Boolean = 3,
    Select  = 4
}

/// <summary>
/// A conditional dependency: this personalization prompt is only shown when
/// the named sibling prompt equals Value.
/// </summary>
public record PersonalizationDependency(string Name, string Value);

/// <summary>An item in a selection-list personalization prompt.</summary>
public record PersonalizationSelectionItem(string Item, string? ImageUrl = null);

/// <summary>
/// A typed, optionally conditional question the agent answers per cart item.
/// Answers are stored in CartItem.PersonalizationAnswers at order time.
/// ChargeAmount is added to the cart total per answered prompt.
/// </summary>
public record PersonalizationPrompt(
    string Name,
    string PromptText,
    bool Required,
    PersonalizationPromptType PromptType,
    decimal ChargeAmount,
    string? Mask = null,
    int MaxLength = 0,
    int DecimalPlaces = 2,
    List<PersonalizationSelectionItem>? SelectionList = null,
    List<PersonalizationDependency>? Dependencies = null);
