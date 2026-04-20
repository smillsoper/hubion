namespace Hubion.Application.Interfaces.Services;

/// <summary>
/// Abstraction over SignalR push — keeps Infrastructure free of API dependencies.
/// Implemented by FlowNotifier in Hubion.Api, which holds IHubContext&lt;FlowHub&gt;.
/// Registered as scoped in Program.cs (after AddSignalR).
/// </summary>
public interface IFlowNotifier
{
    /// <summary>Push the current node state to the agent's SignalR connection.</summary>
    Task PushNodeStateAsync(Guid sessionId, FlowNodeState state, CancellationToken ct = default);

    /// <summary>Push an error to the agent's connection.</summary>
    Task PushErrorAsync(Guid sessionId, string message, CancellationToken ct = default);
}
