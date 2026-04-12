namespace Hubion.Domain.ValueObjects;

/// <summary>
/// Records that a critical action occurred and locks specific fields on the call record.
/// Appended to call_records.commitment_events and call_interactions.commitment_events.
/// The full list of commitment events on a record IS the lock registry — no separate lock table.
/// See ARCHITECTURE.md §21.
/// </summary>
public class CommitmentEvent
{
    public string EventName { get; set; } = string.Empty;
    public string[] LockedFields { get; set; } = [];
    public string LockLabel { get; set; } = string.Empty;
    public bool AllowsSupervisorOverride { get; set; }
    public bool OverrideRequiresReason { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    // Populated when a supervisor approves an override
    public Guid? OverrideApprovedBy { get; set; }
    public string? OverrideReason { get; set; }
    public DateTimeOffset? OverrideApprovedAt { get; set; }
}
