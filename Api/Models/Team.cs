namespace Api.Models;

/// <summary>
/// Abstract base for both static and ephemeral teams
/// Supports flexible team management strategies
/// </summary>
public abstract class Team : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = default!;
    
    public TeamType TeamType { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum TeamType
{
    /// <summary>Roster defined at tournament start, fixed for season</summary>
    Static,
    /// <summary>Teams created on-the-fly for individual events (legacy behavior)</summary>
    Ephemeral
}
