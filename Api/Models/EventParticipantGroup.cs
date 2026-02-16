namespace Api.Models;

public class EventParticipantGroup : AuditableEntity
{
    public Guid ScheduledEventId { get; set; }
    public ScheduledEvent ScheduledEvent { get; set; } = default!;
    
    /// <summary>
    /// Optional: If set, this group uses a Static Team's roster
    /// If null, uses direct EventParticipants (ephemeral/legacy behavior)
    /// </summary>
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
    
    public string Name { get; set; } = default!;
    public int Order { get; set; } // lane/order/team number
    
    /// <summary>
    /// Direct participant assignments (used when Team is null)
    /// Kept for backward compatibility with ephemeral teams
    /// </summary>
    public ICollection<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();
}