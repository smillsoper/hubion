using System.Text.Json.Nodes;
using ContactConnection.Application.Interfaces.Services;

namespace ContactConnection.Infrastructure.FlowEngine.NodeHandlers;

/// <summary>
/// Handles execution of one node type in the flow graph.
/// Each handler is responsible for:
///   1. Resolving template variables in node content
///   2. Applying any input value to the context (for input nodes)
///   3. Determining the next node id (via transition or condition evaluation)
///   4. Returning the node state for SignalR push to the agent UI
/// </summary>
public interface INodeHandler
{
    /// <summary>The node type string this handler processes (e.g. "script", "input").</summary>
    string NodeType { get; }

    /// <summary>
    /// Execute the node and return:
    ///  - The FlowNodeState to push to the agent UI
    ///  - The next node id to advance to (null = terminal)
    /// The handler mutates ctx (adds to Inputs, FlowVars, etc.) as appropriate.
    /// </summary>
    Task<NodeResult> ExecuteAsync(
        JsonObject node,
        FlowExecutionContext ctx,
        string? agentInput,
        string agentTransition,
        CancellationToken ct = default);
}

public record NodeResult(
    FlowNodeState State,
    string? NextNodeId);
