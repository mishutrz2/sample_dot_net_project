namespace Api.Models;

public class Player : AuditableEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = default!;
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;
    public string DisplayName { get; set; } = default!;   
}