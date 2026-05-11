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

    private static string? GetValidationError(
        EmailValidationResult r, bool checkARecord, bool checkMX, bool checkDisposable)
    {
        if (!r.IsFormatValid)
            return "Invalid email format. Please check and try again.";
        if (checkARecord && r.DomainExists == false)
            return "That email domain does not exist. Please check and try again.";
        if (checkMX && r.MXExists == false)
            return "That email domain has no mail server. Please use a valid email address.";
        if (checkDisposable && r.IsDisposable == true)
            return "Disposable email addresses are not accepted. Please use a permanent email address.";
        return null;
    }

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

        // Blank optional — store all vars empty and advance
        if (string.IsNullOrEmpty(email))
        {
            if (!string.IsNullOrEmpty(outputVar))
            {
                ctx.FlowVars[outputVar]                      = string.Empty;
                ctx.FlowVars[$"{outputVar}.isFormatValid"]   = "false";
                ctx.FlowVars[$"{outputVar}.DomainExists"]    = string.Empty;
                ctx.FlowVars[$"{outputVar}.MXExists"]        = string.Empty;
                ctx.FlowVars[$"{outputVar}.isDisposable"]    = string.Empty;
                ctx.FlowVars[$"{outputVar}.isDeliverable"]   = "false";
            }

            var next = Transition(node, agentTransition) ?? Transition(node, "default");
            AppendHistory(ctx, node, email, next);
            return new NodeResult(BuildState(ctx, node, resolvedContent: prompt,
                inputType: "email", required: required), next);
        }

        // Non-blank email — validate and block on any failed check
        var validationResult = await emailValidator.ValidateAsync(
            email, checkARecord, checkMX, checkDisposable, ct);

        // Store vars regardless so downstream branch nodes can use them
        if (!string.IsNullOrEmpty(outputVar))
        {
            ctx.FlowVars[outputVar]                      = email;
            ctx.FlowVars[$"{outputVar}.isFormatValid"]   = validationResult.IsFormatValid ? "true" : "false";
            ctx.FlowVars[$"{outputVar}.DomainExists"]    = validationResult.DomainExists.HasValue
                ? (validationResult.DomainExists.Value ? "true" : "false") : string.Empty;
            ctx.FlowVars[$"{outputVar}.MXExists"]        = validationResult.MXExists.HasValue
                ? (validationResult.MXExists.Value ? "true" : "false") : string.Empty;
            ctx.FlowVars[$"{outputVar}.isDisposable"]    = validationResult.IsDisposable.HasValue
                ? (validationResult.IsDisposable.Value ? "true" : "false") : string.Empty;
            ctx.FlowVars[$"{outputVar}.isDeliverable"]   = validationResult.IsDeliverable ? "true" : "false";
        }

        // Determine error message for the first failing check
        var validationError = GetValidationError(validationResult, checkARecord, checkMX, checkDisposable);
        if (validationError is not null)
        {
            var errorState = BuildState(ctx, node, resolvedContent: prompt,
                inputType: "email", required: required);
            errorState.ValidationError = validationError;
            return new NodeResult(errorState, NextNodeId: null);
        }

        // All checks passed — advance
        var advanceNext = Transition(node, agentTransition) ?? Transition(node, "default");
        AppendHistory(ctx, node, email, advanceNext);
        return new NodeResult(BuildState(ctx, node, resolvedContent: prompt,
            inputType: "email", required: required), advanceNext);
    }
}
