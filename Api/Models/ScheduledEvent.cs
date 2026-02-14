using Api.Models;

namespace Api.Models;

public class ScheduledEvent : AuditableEntity
{
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = "Scheduled";
    public bool IsProjected { get; set; }
    public long Version { get; set; }
    public Guid LeagueId { get; set; }
    public League League { get; set; } = default!;
}