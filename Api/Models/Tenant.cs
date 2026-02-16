namespace Api.Models;

public class Tenant : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    
    public Guid ActivityId { get; set; }
    public Activity Activity { get; set; } = default!;
    
    public TenantVisibility Visibility { get; set; }
    public TenantType Type { get; set; }
    public bool IsActive { get; set; } = true;
    
    public Guid DefaultRoleId { get; set; }
    public Role DefaultRole { get; set; } = default!;

    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    public ICollection<Player> Players { get; set; } = new List<Player>();
    public ICollection<ScheduledEvent> ScheduledEvents { get; set; } = new List<ScheduledEvent>();
}

public enum TenantVisibility
{
    Public,
    LinkOnly,
    Private
}

public enum TenantType
{
    League,
    Tournament,
    Club,
    Community,
    Other
}