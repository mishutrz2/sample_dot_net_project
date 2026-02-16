namespace Api.Models;

/// <summary>
/// Abstraction for retrieving participants from either source
/// Used by services that need participants regardless of team type
/// </summary>
public interface IParticipantProvider
{
    /// <summary>Get all active participants/members for this group</summary>
    Task<IEnumerable<Player>> GetParticipantsAsync();
    
    /// <summary>Is this a static team (persistent roster)?</summary>
    bool IsStaticTeam { get; }
}
