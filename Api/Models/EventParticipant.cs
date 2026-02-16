namespace Api.Models;

public class EventParticipant : Entity
{
    public Guid EventParticipantGroupId { get; set; }
    public EventParticipantGroup EventParticipantGroup { get; set; } = default!;
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = default!;
}