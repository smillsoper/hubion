using System.Text.Json.Nodes;
using ContactConnection.Application.Interfaces.Services;

namespace ContactConnection.Infrastructure.FlowEngine.NodeHandlers;

/// <summary>
/// Handles "set_variable" nodes — assigns a value to a {{flow.*}} variable.
/// Transparent to the agent; executes and advances immediately.
/// Commonly used to extract and store api_call response fields for later use.
///
/// Node schema:
/// {
///   "type": "set_variable",
///   "label": "Store Customer ID",
///   "assignments": [
///     { "variable": "customerId", "value": "{{api.node_005.id}}" },
///     { "variable": "orderTotal",  "value": "{{api.node_005.total_price}}" }
///   ],
///   "transitions": { "default": "node_010" }
/// }
/// </summary>
public class SetVariableNodeHandler(IVariableResolver resolver) : NodeHandlerBase(resolver), INodeHandler
{
    public string NodeType => "set_variable";

    public Task<NodeResult> ExecuteAsync(
        JsonObject node, FlowExecutionContext ctx,
        string? agentInput, string agentTransition, CancellationToken ct = default)
    {
        var varCtx      = ctx.ToVariableContext();
        var assignments = node["assignments"]?.AsArray();

        if (assignments is not null)
        {
            foreach (var item in assignments.OfType<JsonObject>())
            {
                var variable = item["variable"]?.GetValue<string>();
                var template = item["value"]?.GetValue<string>();

                if (variable is null || template is null) continue;

                ctx.FlowVars[variable] = Resolver.Resolve(template, varCtx);
            }
        }

        var next = Transition(node, agentTransition) ?? Transition(node, "default");
        AppendHistory(ctx, node, input: null, transition: next);

        var state = BuildState(ctx, node, resolvedContent: string.Empty);
        return Task.FromResult(new NodeResult(state, next));
    }
}
