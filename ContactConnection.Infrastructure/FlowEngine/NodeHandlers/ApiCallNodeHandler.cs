using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ContactConnection.Application.Interfaces.Services;

namespace ContactConnection.Infrastructure.FlowEngine.NodeHandlers;

/// <summary>
/// Handles "api_call" nodes — makes an HTTP call with template-resolved URL/headers/body.
/// Stores response fields in ctx.ApiResults keyed by "node_id.field" for {{api.node_id.field}}.
/// Commitment events declared in on_success are applied to ctx.LockedFields.
///
/// Node schema:
/// {
///   "type": "api_call",
///   "label": "Shopify Order Lookup",
///   "adapter_id": "uuid",        // platform or tenant adapter (future: resolved from adapter registry)
///   "method": "GET",
///   "url": "https://api.example.com/orders/{{flow.orderId}}",
///   "headers": { "Authorization": "Bearer {{flow.token}}" },
///   "body": "{ \"email\": \"{{call_record.email}}\" }",
///   "response_map": [
///     { "source": "order.id",    "target": "orderId" },
///     { "source": "order.total", "target": "orderTotal" }
///   ],
///   "on_success": {
///     "transition": "node_confirm",
///     "commitment_events": [
///       { "event": "order_submitted", "locks": ["call_record.cart"], "label": "Order submitted" }
///     ]
///   },
///   "on_failure": { "transition": "node_api_error" },
///   "transitions": { "default": "node_confirm" }
/// }
/// </summary>
public class ApiCallNodeHandler(IVariableResolver resolver, IHttpClientFactory httpClientFactory)
    : NodeHandlerBase(resolver), INodeHandler
{
    public string NodeType => "api_call";

    public async Task<NodeResult> ExecuteAsync(
        JsonObject node, FlowExecutionContext ctx,
        string? agentInput, string agentTransition, CancellationToken ct = default)
    {
        var varCtx = ctx.ToVariableContext();
        var nodeId = ctx.CurrentNodeId;

        var method  = Resolver.Resolve(Str(node, "method") ?? "GET", varCtx);
        var url     = Resolver.Resolve(StrReq(node, "url"), varCtx);
        var body    = Str(node, "body") is { } b ? Resolver.Resolve(b, varCtx) : null;

        string? responseText = null;
        string  transitionKey = "default";

        try
        {
            var client  = httpClientFactory.CreateClient("FlowEngine");
            var request = new HttpRequestMessage(new HttpMethod(method), url);

            // Resolve and apply headers
            var headersNode = node["headers"]?.AsObject();
            if (headersNode is not null)
            {
                foreach (var (key, val) in headersNode)
                {
                    var resolvedVal = Resolver.Resolve(val?.GetValue<string>() ?? string.Empty, varCtx);
                    request.Headers.TryAddWithoutValidation(key, resolvedVal);
                }
            }

            if (body is not null)
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, ct);
            responseText = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                // Map response fields into ApiResults
                var responseMap = node["response_map"]?.AsArray();
                if (responseMap is not null && responseText is not null)
                    ApplyResponseMap(nodeId, responseText, responseMap, ctx);

                // Apply commitment events from on_success
                var onSuccess = node["on_success"]?.AsObject();
                if (onSuccess is not null)
                    ApplyCommitmentEvents(onSuccess, ctx);

                transitionKey = onSuccess?["transition"]?.GetValue<string>() ?? "default";
            }
            else
            {
                transitionKey = "on_failure";
            }
        }
        catch
        {
            transitionKey = "on_failure";
        }

        // Store raw response text for debugging / response_map miss recovery
        ctx.ApiResults[$"{nodeId}._raw"] = responseText ?? string.Empty;

        var next = Transition(node, transitionKey) ?? Transition(node, "default");
        AppendHistory(ctx, node, input: null, transition: next);

        var state = BuildState(ctx, node, resolvedContent: string.Empty);
        return new NodeResult(state, next);
    }

    private static void ApplyResponseMap(
        string nodeId, string responseText, JsonArray responseMap, FlowExecutionContext ctx)
    {
        JsonNode? responseJson;
        try { responseJson = JsonNode.Parse(responseText); }
        catch { return; }

        foreach (var item in responseMap.OfType<JsonObject>())
        {
            var source = item["source"]?.GetValue<string>();
            var target = item["target"]?.GetValue<string>();
            if (source is null || target is null) continue;

            var value = GetNestedValue(responseJson, source);
            if (value is not null)
                ctx.ApiResults[$"{nodeId}.{target}"] = value;
        }
    }

    private static string? GetNestedValue(JsonNode? node, string dotPath)
    {
        var parts = dotPath.Split('.');
        var current = node;
        foreach (var part in parts)
        {
            current = current?[part];
            if (current is null) return null;
        }
        return current?.ToString();
    }

    private static void ApplyCommitmentEvents(JsonObject onSuccess, FlowExecutionContext ctx)
    {
        var events = onSuccess["commitment_events"]?.AsArray();
        if (events is null) return;

        foreach (var evt in events.OfType<JsonObject>())
        {
            var locks = evt["locks"]?.AsArray();
            if (locks is null) continue;
            foreach (var lockField in locks)
            {
                var field = lockField?.GetValue<string>();
                if (field is not null) ctx.LockedFields.Add(field);
            }
        }
    }
}
