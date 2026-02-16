using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

/// <summary>
/// Service for resolving participants in an event group
/// Abstracts away the complexity of static vs ephemeral teams
/// This is the KEY to supporting both team types with minimal code changes
/// </summary>
public interface IEventParticipantService
{
    /// <summary>
    /// Gets all active participants for an event group
    /// Automatically resolves from either Static Team or direct EventParticipants
    /// </summary>
    Task<IEnumerable<Player>> GetParticipantsAsync(Guid eventParticipantGroupId);
    
    /// <summary>Get participant count without loading all players</summary>
    Task<int> GetParticipantCountAsync(Guid eventParticipantGroupId);
    
    /// <summary>Check if this group uses a static team</summary>
    Task<bool> IsStaticTeamAsync(Guid eventParticipantGroupId);
}

public class EventParticipantService : IEventParticipantService
{
    private readonly ApplicationDbContext _context;

    public EventParticipantService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all participants - handles both static and ephemeral teams transparently
    /// The calling code doesn't need to know which type it is!
    /// </summary>
    public async Task<IEnumerable<Player>> GetParticipantsAsync(Guid eventParticipantGroupId)
    {
        var group = await _context.EventParticipantGroups
            .Include(epg => epg.Team)
            .Include(epg => epg.EventParticipants)
                .ThenInclude(ep => ep.Player)
            .FirstOrDefaultAsync(epg => epg.Id == eventParticipantGroupId);

        if (group == null)
            return Enumerable.Empty<Player>();

        // Strategy: Use Static Team roster if available
        if (group.TeamId.HasValue && group.Team is StaticTeam staticTeam)
        {
            // Get current roster (members whose LeftAt is null)
            var currentMembers = await _context.StaticTeamMembers
                .Where(stm => stm.StaticTeamId == staticTeam.Id && stm.LeftAt == null)
                .Include(stm => stm.Player)
                .Select(stm => stm.Player)
                .ToListAsync();

            return currentMembers;
        }

        // Fallback: Use direct EventParticipants (ephemeral teams)
        return group.EventParticipants
            .Select(ep => ep.Player)
            .ToList();
    }

    /// <summary>
    /// Get participant count efficiently
    /// Avoids loading full player objects when only count is needed
    /// </summary>
    public async Task<int> GetParticipantCountAsync(Guid eventParticipantGroupId)
    {
        var group = await _context.EventParticipantGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(epg => epg.Id == eventParticipantGroupId);

        if (group == null)
            return 0;

        // Static team: count active members
        if (group.TeamId.HasValue)
        {
            return await _context.StaticTeamMembers
                .Where(stm => stm.StaticTeamId == group.TeamId && stm.LeftAt == null)
                .CountAsync();
        }

        // Ephemeral team: count direct participants
        return await _context.EventParticipants
            .Where(ep => ep.EventParticipantGroupId == eventParticipantGroupId)
            .CountAsync();
    }

    /// <summary>
    /// Check if this group uses a static team
    /// </summary>
    public async Task<bool> IsStaticTeamAsync(Guid eventParticipantGroupId)
    {
        return await _context.EventParticipantGroups
            .AsNoTracking()
            .Where(epg => epg.Id == eventParticipantGroupId && epg.TeamId.HasValue)
            .AnyAsync();
    }
}
