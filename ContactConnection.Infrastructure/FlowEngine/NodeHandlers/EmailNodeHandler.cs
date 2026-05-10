using System.Text.Json.Nodes;
using ContactConnection.Application.Interfaces.Services;

namespace ContactConnection.Infrastructure.FlowEngine.NodeHandlers;

/// <summary>
/// Handles "email" nodes — agent captures and validates an email address.
///
/// Node schema:
/// {
///   "type": "email",
///   "label": "Customer Email",
///   "outputVariable": "customer_email",
///   "required": true,
///   "checkARecord": true,
///   "checkMX": true,
///   "checkDisposable": true,
///   "transitions": { "default": "node_002" }
/// }
///
/// Emits flat-key flow variables (resolved via {{flow.customer_email.isDeliverable}}):
///   {outputVar}              = the raw email value
///   {outputVar}.isFormatValid
///   {outputVar}.DomainExists   (empty string if checkARecord=false)
///   {outputVar}.MXExists       (empty string if checkMX=false)
///   {outputVar}.isDisposable   (empty string if checkDisposable=false)
///   {outputVar}.isDeliverable
/// </summary>
public class EmailNodeHandler(IVariableResolver resolver, IEmailValidationService emailValidator)
    : NodeHandlerBase(resolver), INodeHandler
{
    public string NodeType => "email";

    public async Task<NodeResult> ExecuteAsync(
        JsonObject node, FlowExecutionContext ctx,
        string? agentInput, string agentTransition, CancellationToken ct = default)
    {
        var required        = node["required"]?.GetValue<bool>() ?? false;
        var checkARecord    = node["checkARecord"]?.GetValue<bool>() ?? false;
        var checkMX         = node["checkMX"]?.GetValue<bool>() ?? false;
        var checkDisposable = node["checkDisposable"]?.GetValue<bool>() ?? false;
        var outputVar       = Str(node, "outputVariable")?.Trim() ?? string.Empty;
        var prompt          = Resolver.Resolve(Str(node, "prompt") ?? string.Empty, ctx.ToVariableContext());

        // First display (no input yet)
        if (agentInput is null)
        {
            var display = BuildState(ctx, node, resolvedContent: prompt,
                inputType: "email", required: required);
            return new NodeResult(display, NextNodeId: null);
        }

        var email = agentInput.Trim();

        // Required guard — re-display if blank
        if (required && string.IsNullOrEmpty(email))
        {
            var display = BuildState(ctx, node, resolvedContent: prompt,
                inputType: "email", required: required);
            return new NodeResult(display, NextNodeId: null);
        }

        // Store sub-property variables
        if (!string.IsNullOrEmpty(outputVar))
        {
            if (string.IsNullOrEmpty(email))
            {
                // Blank optional — store empty / false for all sub-props
                ctx.FlowVars[outputVar]                      = string.Empty;
                ctx.FlowVars[$"{outputVar}.isFormatValid"]   = "false";
                ctx.FlowVars[$"{outputVar}.DomainExists"]    = string.Empty;
                ctx.FlowVars[$"{outputVar}.MXExists"]        = string.Empty;
                ctx.FlowVars[$"{outputVar}.isDisposable"]    = string.Empty;
                ctx.FlowVars[$"{outputVar}.isDeliverable"]   = "false";
            }
            else
            {
                var result = await emailValidator.ValidateAsync(
                    email, checkARecord, checkMX, checkDisposable, ct);

                ctx.FlowVars[outputVar]                      = email;
                ctx.FlowVars[$"{outputVar}.isFormatValid"]   = result.IsFormatValid ? "true" : "false";
                ctx.FlowVars[$"{outputVar}.DomainExists"]    = result.DomainExists.HasValue
                    ? (result.DomainExists.Value ? "true" : "false") : string.Empty;
                ctx.FlowVars[$"{outputVar}.MXExists"]        = result.MXExists.HasValue
                    ? (result.MXExists.Value ? "true" : "false") : string.Empty;
                ctx.FlowVars[$"{outputVar}.isDisposable"]    = result.IsDisposable.HasValue
                    ? (result.IsDisposable.Value ? "true" : "false") : string.Empty;
                ctx.FlowVars[$"{outputVar}.isDeliverable"]   = result.IsDeliverable ? "true" : "false";
            }
        }

        var next = Transition(node, agentTransition) ?? Transition(node, "default");
        AppendHistory(ctx, node, email, next);

        var state = BuildState(ctx, node, resolvedContent: prompt,
            inputType: "email", required: required);
        return new NodeResult(state, next);
    }
}
