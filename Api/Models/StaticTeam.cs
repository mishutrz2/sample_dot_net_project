namespace Api.Models;

/// <summary>
/// A permanent team that exists independent of events
/// Has a defined roster that evolves over time through transfers
/// </summary>
public class StaticTeam : Team
{
    /// <summary>Persistent roster members with transfer history</summary>
    public ICollection<StaticTeamMember> Members { get; set; } = new List<StaticTeamMember>();
    
    /// <summary>Season/competition period</summary>
    public DateTime SeasonStartDate { get; set; }
    public DateTime? SeasonEndDate { get; set; }
}

/// <summary>
/// Tracks individual player's membership in a static team
/// Records transfers, loans, etc.
/// </summary>
public class StaticTeamMember : Entity
{
    public Guid Id { get; set; } = Guid.CreateVersion7(); // Unique ID for transfer history
    public Guid StaticTeamId { get; set; }
    public StaticTeam StaticTeam { get; set; } = null!;
    
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    
    /// <summary>When player joined this team</summary>
    public DateTime JoinedAt { get; set; }
    
    /// <summary>When player left (transfer/removal). Null = still active</summary>
    public DateTime? LeftAt { get; set; }
    
    /// <summary>Reason for leaving (Transfer, Removed, Loan, etc.)</summary>
    public string? LeaveReason { get; set; }
}
