using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class AppUser : AuditableEntity
{
    public string AwsSubject { get; set; } = default!;
    
    [EmailAddress]
    public string Email { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = default!;
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
} 