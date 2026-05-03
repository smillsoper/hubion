namespace ContactConnection.Application.Interfaces.Services;

/// <summary>
/// The server-side flow interpreter. Manages session lifecycle, node dispatch,
/// variable resolution, and SignalR push. All agent interaction goes through here.
/// </summary>
public interface IFlowEngine
{
    /// <summary>
    /// Starts a new flow session against an existing call record and interaction.
    /// Returns the initial node state to push to the agent UI.
    /// </summary>
    Task<FlowNodeState> StartAsync(StartFlowRequest request, CancellationToken ct = default);

    /// <summary>
    /// Advances the session to the next node based on agent input.
    /// Returns the next node state, or a terminal state if the flow has ended.
    /// </summary>
    Task<FlowNodeState> AdvanceAsync(AdvanceFlowRequest request, CancellationToken ct = default);

    /// <summary>
    /// Returns the current node state for an active session (e.g. on reconnect).
    /// </summary>
    Task<FlowNodeState?> GetCurrentStateAsync(Guid sessionId, CancellationToken ct = default);
}

public class StartFlowRequest
{
    public required Guid FlowId { get; init; }
    public required Guid CallRecordId { get; init; }
    public required Guid InteractionId { get; init; }
    public required Guid AgentId { get; init; }
    public required Guid TenantId { get; init; }
}

public class AdvanceFlowRequest
{
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Agent's input for the current node (text entered, option selected, etc.).
    /// Null for nodes that don't capture input (script nodes advanced by button click).
    /// </summary>
    public string? InputValue { get; init; }

    /// <summary>
    /// For branch nodes: which transition the agent chose (or the engine evaluated).
    /// For most nodes this is "default".
    /// </summary>
    public string Transition { get; init; } = "default";
}

/// <summary>
/// The state of a node after execution — sent to the agent UI via SignalR.
/// The UI is a thin renderer: it displays what the engine says to display.
/// </summary>
public class FlowNodeState
{
    public required Guid SessionId { get; init; }
    public required string NodeId { get; init; }
    public required string NodeType { get; init; }   // script | input | branch | end | ...
    public required string Label { get; init; }

    /// <summary>Resolved content for display (script text with tags substituted).</summary>
    public string? Content { get; init; }

    /// <summary>For input nodes: the type of input expected.</summary>
    public string? InputType { get; init; }          // text | select | checkbox | date | address | phone

    /// <summary>For select/radio input nodes: the available options.</summary>
    public List<FlowOption>? Options { get; init; }

    /// <summary>For branch nodes: the condition text (informational for supervisor view).</summary>
    public string? Condition { get; init; }

    /// <summary>True when the flow has reached an end node.</summary>
    public bool IsTerminal { get; init; }

    /// <summary>Locked fields from commitment events — UI renders these as locked.</summary>
    public List<string> LockedFields { get; init; } = [];
}

public class FlowOption
{
    public required string Value { get; init; }
    public required string Label { get; init; }
}
