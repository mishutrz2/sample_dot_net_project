namespace Api.Models;

public class Membership : Entity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = null!;
    
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public MembershipStatus Status { get; set; }


}

public enum MembershipStatus
{
    Active,
    Inactive,
    Pending,
    Banned
}