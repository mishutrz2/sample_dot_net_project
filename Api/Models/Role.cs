namespace Api.Models;

public class Role : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsDefault { get; set; } = false;
    
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
}