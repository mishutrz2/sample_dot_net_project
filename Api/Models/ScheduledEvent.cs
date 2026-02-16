using Api.Models;

namespace Api.Models;

public class ScheduledEvent : AuditableEntity
{
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public EventStatus Status { get; set; }
    public EventType Type { get; set; }
    public bool IsProjected { get; set; }
    public long Version { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = default!;
}

public enum EventStatus
{
    Scheduled,
    Ongoing,
    Completed,
    Cancelled
}

public enum EventType
{
    Match,
    Practice,
    Challenge,
    Other
}