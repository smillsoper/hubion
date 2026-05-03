using System.Text.Json.Nodes;
using ContactConnection.Application.Interfaces.Services;

namespace ContactConnection.Infrastructure.FlowEngine.NodeHandlers;

/// <summary>
/// Handles "input" nodes — agent captures a value (text, select, checkbox, date, address, phone).
/// The captured value is stored in ctx.Inputs keyed by node_id for later {{input.node_id}} resolution.
///
/// Node schema:
/// {
///   "type": "input",
///   "label": "First Name",
///   "input_type": "text" | "select" | "checkbox" | "date" | "address" | "phone",
///   "prompt": "Ask the caller for their first name.",
///   "options": [{ "value": "yes", "label": "Yes" }, ...],   // for select/radio
///   "required": true,
///   "transitions": { "default": "node_002" }
/// }
/// </summary>
public class InputNodeHandler(IVariableResolver resolver) : NodeHandlerBase(resolver), INodeHandler
{
    public string NodeType => "input";

    public Task<NodeResult> ExecuteAsync(
        JsonObject node, FlowExecutionContext ctx,
        string? agentInput, string agentTransition, CancellationToken ct = default)
    {
        var varCtx    = ctx.ToVariableContext();
        var inputType = Str(node, "input_type") ?? "text";
        var prompt    = Resolver.Resolve(Str(node, "prompt") ?? string.Empty, varCtx);

        // If agent has submitted a value, store it and advance
        if (agentInput is not null)
        {
            // Guard: locked field check (field name = node label normalized)
            var fieldKey = Str(node, "field_key");
            if (fieldKey is not null && ctx.LockedFields.Contains(fieldKey))
                throw new InvalidOperationException(
                    $"Field '{fieldKey}' is locked by a commitment event and cannot be modified.");

            ctx.Inputs[ctx.CurrentNodeId] = agentInput;

            var next = Transition(node, agentTransition) ?? Transition(node, "default");
            AppendHistory(ctx, node, agentInput, next);

            var advancedState = BuildState(ctx, node, resolvedContent: prompt,
                inputType: inputType, options: ParseOptions(node));
            return Task.FromResult(new NodeResult(advancedState, next));
        }

        // No input yet — return the node for display (agent must submit before advancing)
        var displayState = BuildState(ctx, node, resolvedContent: prompt,
            inputType: inputType, options: ParseOptions(node));
        return Task.FromResult(new NodeResult(displayState, NextNodeId: null));
    }

    private static List<FlowOption>? ParseOptions(JsonObject node)
    {
        var optionsArray = node["options"]?.AsArray();
        if (optionsArray is null) return null;

        return optionsArray
            .OfType<JsonObject>()
            .Select(o => new FlowOption
            {
                Value = o["value"]?.GetValue<string>() ?? string.Empty,
                Label = o["label"]?.GetValue<string>() ?? string.Empty
            })
            .ToList();
    }
}
