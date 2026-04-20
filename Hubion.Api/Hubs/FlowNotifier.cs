using Hubion.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace Hubion.Api.Hubs;

/// <summary>
/// IFlowNotifier implementation — wraps IHubContext to push node state to agent UI.
/// Registered in Program.cs after AddSignalR so the Hub types are available.
/// Infrastructure never references this class directly — it depends on IFlowNotifier only.
/// </summary>
public class FlowNotifier(IHubContext<FlowHub, IFlowHubClient> hubContext) : IFlowNotifier
{
    public Task PushNodeStateAsync(Guid sessionId, FlowNodeState state, CancellationToken ct = default) =>
        hubContext.Clients.Group($"session:{sessionId}").ReceiveNodeState(state);

    public Task PushErrorAsync(Guid sessionId, string message, CancellationToken ct = default) =>
        hubContext.Clients.Group($"session:{sessionId}").ReceiveError(message);
}
