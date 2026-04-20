using System.Text.Json;
using System.Text.Json.Nodes;
using Hubion.Application.Interfaces.Repositories;
using Hubion.Application.Interfaces.Services;
using Hubion.Application.Services;
using Hubion.Domain.Entities;
using Hubion.Infrastructure.FlowEngine.NodeHandlers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Hubion.Infrastructure.FlowEngine;

/// <summary>
/// Server-side flow interpreter.
///
/// Execution loop:
///   1. Load FlowExecutionContext from Redis (active) or build fresh (start)
///   2. Dispatch to the correct INodeHandler
///   3. If the handler returns a next node immediately (branch, set_variable, api_call)
///      keep advancing until a node requires agent interaction or the flow ends
///   4. Save context back to Redis
///   5. On terminal node: persist FlowSession to PostgreSQL, remove from Redis
/// </summary>
public class FlowEngine : IFlowEngine
{
    private readonly IFlowRepository _flows;
    private readonly IFlowSessionRepository _sessions;
    private readonly IDatabase _redis;
    private readonly TenantContext _tenantContext;
    private readonly IFlowNotifier _notifier;
    private readonly Dictionary<string, INodeHandler> _handlers;
    private readonly ILogger<FlowEngine> _logger;

    // Transparent node types — engine auto-advances without waiting for agent input
    private static readonly HashSet<string> AutoAdvanceTypes =
        ["branch", "set_variable"];

    private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(12);

    public FlowEngine(
        IFlowRepository flows,
        IFlowSessionRepository sessions,
        IConnectionMultiplexer redis,
        TenantContext tenantContext,
        IFlowNotifier notifier,
        IEnumerable<INodeHandler> handlers,
        ILogger<FlowEngine> logger)
    {
        _flows         = flows;
        _sessions      = sessions;
        _redis         = redis.GetDatabase();
        _tenantContext = tenantContext;
        _notifier      = notifier;
        _handlers      = handlers.ToDictionary(h => h.NodeType, StringComparer.OrdinalIgnoreCase);
        _logger        = logger;
    }

    public async Task<FlowNodeState> StartAsync(StartFlowRequest request, CancellationToken ct = default)
    {
        var flow = await _flows.GetByIdAsync(request.FlowId, ct)
            ?? throw new InvalidOperationException($"Flow {request.FlowId} not found.");

        if (!flow.IsActive)
            throw new InvalidOperationException($"Flow {request.FlowId} is not published.");

        var definition = JsonNode.Parse(flow.Definition)?.AsObject()
            ?? throw new InvalidOperationException("Flow definition is invalid JSON.");

        var entryNodeId = definition["entry_node"]?.GetValue<string>()
            ?? throw new InvalidOperationException("Flow definition has no entry_node.");

        var session = FlowSession.Create(
            tenantId:      request.TenantId,
            flowId:        flow.Id,
            flowVersion:   flow.Version,
            callRecordId:  request.CallRecordId,
            interactionId: request.InteractionId,
            agentId:       request.AgentId,
            entryNodeId:   entryNodeId);

        await _sessions.AddAsync(session, ct);
        await _sessions.SaveChangesAsync(ct);

        var ctx   = BuildContext(session, definition, request);
        await SaveToRedis(ctx, ct);

        var state = await AdvanceInternalAsync(ctx, entryNodeId, agentInput: null, transition: "default", isStart: true, ct);
        await _notifier.PushNodeStateAsync(session.Id, state, ct);
        return state;
    }

    public async Task<FlowNodeState> AdvanceAsync(AdvanceFlowRequest request, CancellationToken ct = default)
    {
        var ctx = await LoadFromRedis(request.SessionId, ct)
            ?? throw new InvalidOperationException($"No active session {request.SessionId}.");

        var state = await AdvanceInternalAsync(
            ctx, ctx.CurrentNodeId, request.InputValue, request.Transition, isStart: false, ct);

        if (state.IsTerminal)
            await CompleteSession(ctx, ct);
        else
            await SaveToRedis(ctx, ct);

        await _notifier.PushNodeStateAsync(request.SessionId, state, ct);
        return state;
    }

    public async Task<FlowNodeState?> GetCurrentStateAsync(Guid sessionId, CancellationToken ct = default)
    {
        var ctx = await LoadFromRedis(sessionId, ct);
        if (ctx is null) return null;

        var node = GetNode(ctx.FlowDefinition, ctx.CurrentNodeId);
        if (node is null) return null;

        var nodeType = node["type"]?.GetValue<string>() ?? "script";
        if (!_handlers.TryGetValue(nodeType, out var handler)) return null;

        var result = await handler.ExecuteAsync(node, ctx, agentInput: null, agentTransition: "default", ct);
        return result.State;
    }

    // ── Internal engine loop ────────────────────────────────────────────────

    private async Task<FlowNodeState> AdvanceInternalAsync(
        FlowExecutionContext ctx, string nodeId,
        string? agentInput, string transition, bool isStart, CancellationToken ct)
    {
        const int MaxAutoAdvance = 50; // safety cap — prevents infinite loops in bad flow definitions
        var steps = 0;

        while (true)
        {
            ctx.CurrentNodeId = nodeId;
            var node = GetNode(ctx.FlowDefinition, nodeId)
                ?? throw new InvalidOperationException($"Node '{nodeId}' not found in flow definition.");

            var nodeType = node["type"]?.GetValue<string>()
                ?? throw new InvalidOperationException($"Node '{nodeId}' has no type.");

            if (!_handlers.TryGetValue(nodeType, out var handler))
                throw new InvalidOperationException($"No handler registered for node type '{nodeType}'.");

            var result = await handler.ExecuteAsync(node, ctx, agentInput, transition, ct);

            // Terminal node or node waiting for input — return state as-is
            if (result.NextNodeId is null || result.State.IsTerminal)
                return result.State;

            // Auto-advancing node (branch, set_variable) — loop immediately without waiting for agent
            if (AutoAdvanceTypes.Contains(nodeType) && ++steps < MaxAutoAdvance)
            {
                nodeId      = result.NextNodeId;
                agentInput  = null;
                transition  = "default";
                continue;
            }

            // StartAsync: show the first interactive node to the agent — stop here.
            // ctx.CurrentNodeId is already set to nodeId at the top of the loop.
            if (isStart)
                return result.State;

            // AdvanceAsync: agent acted on this node — advance past it to the next node.
            // The loop will process the next node; if it's an input waiting for data it'll
            // return immediately with NextNodeId=null, leaving the cursor there.
            nodeId     = result.NextNodeId;
            agentInput = null;
            transition = "default";
            isStart    = false; // already false, but keep explicit
        }
    }

    // ── Context build/persist ───────────────────────────────────────────────

    private static FlowExecutionContext BuildContext(
        FlowSession session, JsonObject definition, StartFlowRequest request)
    {
        return new FlowExecutionContext
        {
            SessionId      = session.Id,
            FlowId         = session.FlowId,
            FlowVersion    = session.FlowVersion,
            CallRecordId   = session.CallRecordId,
            InteractionId  = session.InteractionId,
            AgentId        = session.AgentId,
            TenantId       = session.TenantId,
            CurrentNodeId  = session.CurrentNodeId,
            FlowDefinition = definition,
            // Caller/agent/tenant data populated from call record at session start.
            // In production these come from the call record lookup — stubbed here
            // until FreeSWITCH screen pop populates CallDetail fields.
            CallRecord = [],
            Caller     = [],
            Agent      = new() { ["id"] = request.AgentId.ToString() },
            Tenant     = new() { ["id"] = request.TenantId.ToString() }
        };
    }

    private async Task SaveToRedis(FlowExecutionContext ctx, CancellationToken ct)
    {
        var key   = RedisKey(ctx.SessionId);
        var value = JsonSerializer.Serialize(new RedisCacheEntry(ctx));
        await _redis.StringSetAsync(key, value, SessionTtl);
    }

    private async Task<FlowExecutionContext?> LoadFromRedis(Guid sessionId, CancellationToken ct)
    {
        var key  = RedisKey(sessionId);
        var json = await _redis.StringGetAsync(key);
        if (!json.HasValue) return null;

        var entry = JsonSerializer.Deserialize<RedisCacheEntry>(json.ToString());
        if (entry is null) return null;

        // Re-parse definition (stored as string in cache entry)
        var definition = JsonNode.Parse(entry.DefinitionJson)?.AsObject() ?? [];

        return FlowExecutionContext.Deserialize(
            sessionId:          sessionId,
            flowId:             entry.FlowId,
            flowVersion:        entry.FlowVersion,
            callRecordId:       entry.CallRecordId,
            interactionId:      entry.InteractionId,
            agentId:            entry.AgentId,
            tenantId:           entry.TenantId,
            currentNodeId:      entry.CurrentNodeId,
            definitionJson:     entry.DefinitionJson,
            variableStoreJson:  entry.VariableStoreJson,
            executionHistoryJson: entry.ExecutionHistoryJson,
            callRecord:         entry.CallRecord,
            caller:             entry.Caller,
            agent:              entry.Agent,
            tenant:             entry.Tenant);
    }

    private async Task CompleteSession(FlowExecutionContext ctx, CancellationToken ct)
    {
        // Persist final state to PostgreSQL
        var session = await _sessions.GetByIdAsync(ctx.SessionId, ct);
        if (session is not null)
        {
            session.Complete(ctx.SerializeVariableStore(), ctx.SerializeExecutionHistory());
            await _sessions.SaveChangesAsync(ct);
        }

        // Remove from Redis
        await _redis.KeyDeleteAsync(RedisKey(ctx.SessionId));
    }

    private static JsonObject? GetNode(JsonObject definition, string nodeId) =>
        definition["nodes"]?[nodeId]?.AsObject();

    private static string RedisKey(Guid sessionId) => $"flow:session:{sessionId}";

    // Compact Redis cache entry — avoids re-querying PostgreSQL on every advance
    private record RedisCacheEntry(
        Guid FlowId, int FlowVersion,
        Guid CallRecordId, Guid InteractionId, Guid AgentId, Guid TenantId,
        string CurrentNodeId, string DefinitionJson,
        string VariableStoreJson, string ExecutionHistoryJson,
        Dictionary<string, string> CallRecord,
        Dictionary<string, string> Caller,
        Dictionary<string, string> Agent,
        Dictionary<string, string> Tenant)
    {
        // Parameterless ctor for deserialization
        public RedisCacheEntry() : this(Guid.Empty, 0, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty,
            string.Empty, "{}", "{}", "[]", [], [], [], []) { }

        public RedisCacheEntry(FlowExecutionContext ctx) : this(
            ctx.FlowId, ctx.FlowVersion,
            ctx.CallRecordId, ctx.InteractionId, ctx.AgentId, ctx.TenantId,
            ctx.CurrentNodeId,
            ctx.FlowDefinition.ToJsonString(),
            ctx.SerializeVariableStore(),
            ctx.SerializeExecutionHistory(),
            ctx.CallRecord, ctx.Caller, ctx.Agent, ctx.Tenant) { }
    }
}
