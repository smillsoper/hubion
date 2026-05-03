using System.Text.Json.Nodes;
using ContactConnection.Application.Interfaces.Services;

namespace ContactConnection.Infrastructure.FlowEngine.NodeHandlers;

/// <summary>
/// Handles "branch" nodes — evaluates a condition and follows the matching transition.
/// Branches are transparent to the agent: the engine evaluates and advances immediately.
///
/// Node schema:
/// {
///   "type": "branch",
///   "label": "Existing Customer?",
///   "condition": "{{call_record.account_number}} != \"\"",
///   "transitions": {
///     "true":    "node_existing",
///     "false":   "node_new",
///     "default": "node_new"     // fallback if condition evaluation fails
///   }
/// }
///
/// Condition operators: == != > < >= <= contains
/// </summary>
public class BranchNodeHandler(IVariableResolver resolver) : NodeHandlerBase(resolver), INodeHandler
{
    public string NodeType => "branch";

    public Task<NodeResult> ExecuteAsync(
        JsonObject node, FlowExecutionContext ctx,
        string? agentInput, string agentTransition, CancellationToken ct = default)
    {
        var varCtx    = ctx.ToVariableContext();
        var condition = Str(node, "condition") ?? "true";

        bool result;
        try
        {
            result = Resolver.EvaluateCondition(condition, varCtx);
        }
        catch
        {
            // Condition evaluation failure → take default path
            result = false;
        }

        var transitionKey = result ? "true" : "false";
        var next = Transition(node, transitionKey) ?? Transition(node, "default");

        AppendHistory(ctx, node, input: null, transition: next);

        // Branch nodes don't display to the agent — they advance immediately.
        // The state is still returned for supervisor audit visibility.
        var state = BuildState(ctx, node,
            resolvedContent: string.Empty,
            condition: $"{condition} → {transitionKey}");

        return Task.FromResult(new NodeResult(state, next));
    }
}
