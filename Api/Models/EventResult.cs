namespace Api.Models;

public class EventResult : AuditableEntity
{
    /// <summary>
    /// The scheduled event this result belongs to
    /// </summary>
    public Guid ScheduledEventId { get; set; }
    public ScheduledEvent ScheduledEvent { get; set; } = default!;

    /// <summary>
    /// ID of the winning group/team (null if draw)
    /// </summary>
    public Guid? WinningGroupId { get; set; }
    public EventParticipantGroup? WinningGroup { get; set; }

    /// <summary>
    /// Overall result status of the event
    /// </summary>
    public EventResultStatus Status { get; set; }

    /// <summary>
    /// Timestamp when the event completed (can differ from ScheduledEvent.EndTime)
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Optional notes about the result (injuries, disqualifications, etc.)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// JSON data for flexible score/result storage
    /// Example: { "groupScores": { "group1": 3, "group2": 2 } }
    /// </summary>
    public string? ResultData { get; set; }
}

public enum EventResultStatus
{
    Draw,
    HasWinner,
    Cancelled,
    NoResult,
    Disputed
}
