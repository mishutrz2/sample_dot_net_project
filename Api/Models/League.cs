namespace Api.Models;

public class League : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Visibility { get; set; } = "Private";
    public Guid ActivityId { get; set; }
    public Activity Activity { get; set; } = default!;
    public ICollection<ScheduledEvent> Events { get; set; } = new List<ScheduledEvent>();
    public ICollection<AppUserLeague> AppUserLeagues { get; set; } = new List<AppUserLeague>();
}