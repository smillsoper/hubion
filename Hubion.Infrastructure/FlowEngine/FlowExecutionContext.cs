using System.Text.Json;
using System.Text.Json.Nodes;
using Hubion.Application.Interfaces.Services;

namespace Hubion.Infrastructure.FlowEngine;

/// <summary>
/// Runtime state for one active flow session. Loaded from Redis on each request,
/// serialized back to Redis after each advance. Written to PostgreSQL on completion.
/// </summary>
public class FlowExecutionContext
{
    public Guid SessionId { get; init; }
    public Guid FlowId { get; init; }
    public int FlowVersion { get; init; }
    public Guid CallRecordId { get; init; }
    public Guid InteractionId { get; init; }
    public Guid AgentId { get; init; }
    public Guid TenantId { get; init; }
    public string CurrentNodeId { get; set; } = string.Empty;

    // The full flow definition JSON, parsed once at session start
    public JsonObject FlowDefinition { get; init; } = [];

    // Variable state — projected into VariableContext for resolver calls
    public Dictionary<string, string> FlowVars { get; init; } = [];
    public Dictionary<string, string> Inputs { get; init; } = [];
    public Dictionary<string, string> ApiResults { get; init; } = [];

    // Baseline data populated from call record and agent at session start
    public Dictionary<string, string> CallRecord { get; init; } = [];
    public Dictionary<string, string> Caller { get; init; } = [];
    public Dictionary<string, string> Agent { get; init; } = [];
    public Dictionary<string, string> Tenant { get; init; } = [];

    // Append-only execution log — every node visited with timestamp and input
    public List<NodeExecutionRecord> ExecutionHistory { get; init; } = [];

    // Locked fields from commitment events — enforced at engine and API layers
    public HashSet<string> LockedFields { get; init; } = [];

    // Serialization helpers ─────────────────────────────────────────────────

    public VariableContext ToVariableContext() => new()
    {
        CallRecord = CallRecord,
        Caller     = Caller,
        Agent      = Agent,
        Tenant     = Tenant,
        Inputs     = Inputs,
        ApiResults = ApiResults,
        FlowVars   = FlowVars
    };

    public string SerializeVariableStore() =>
        JsonSerializer.Serialize(new { FlowVars, Inputs, ApiResults });

    public string SerializeExecutionHistory() =>
        JsonSerializer.Serialize(ExecutionHistory);

    public static FlowExecutionContext Deserialize(
        Guid sessionId, Guid flowId, int flowVersion,
        Guid callRecordId, Guid interactionId, Guid agentId, Guid tenantId,
        string currentNodeId, string definitionJson,
        string variableStoreJson, string executionHistoryJson,
        Dictionary<string, string> callRecord,
        Dictionary<string, string> caller,
        Dictionary<string, string> agent,
        Dictionary<string, string> tenant)
    {
        var varStore = JsonSerializer.Deserialize<VariableStore>(variableStoreJson)
            ?? new VariableStore();
        var history  = JsonSerializer.Deserialize<List<NodeExecutionRecord>>(executionHistoryJson)
            ?? [];
        var definition = JsonNode.Parse(definitionJson)?.AsObject() ?? [];

        return new FlowExecutionContext
        {
            SessionId        = sessionId,
            FlowId           = flowId,
            FlowVersion      = flowVersion,
            CallRecordId     = callRecordId,
            InteractionId    = interactionId,
            AgentId          = agentId,
            TenantId         = tenantId,
            CurrentNodeId    = currentNodeId,
            FlowDefinition   = definition,
            FlowVars         = varStore.FlowVars,
            Inputs           = varStore.Inputs,
            ApiResults       = varStore.ApiResults,
            CallRecord       = callRecord,
            Caller           = caller,
            Agent            = agent,
            Tenant           = tenant,
            ExecutionHistory = history
        };
    }

    private record VariableStore(
        Dictionary<string, string> FlowVars,
        Dictionary<string, string> Inputs,
        Dictionary<string, string> ApiResults)
    {
        public VariableStore() : this([], [], []) { }
    }
}

/// <summary>
/// One entry in the execution history — the immutable audit trail of node traversal.
/// </summary>
public record NodeExecutionRecord(
    string NodeId,
    string NodeType,
    string Label,
    DateTimeOffset EnteredAt,
    string? InputValue,
    string? TransitionTaken);
