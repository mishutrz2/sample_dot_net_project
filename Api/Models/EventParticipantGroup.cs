namespace Api.Models;

public class EventParticipantGroup : AuditableEntity
{
    public Guid ScheduledEventId { get; set; }
    public ScheduledEvent ScheduledEvent { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int Order { get; set; } // lane/order/team number
}