using Api.Data;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class LeagueService(ApplicationDbContext _context) : ILeagueService
{
    public async Task<IEnumerable<League>> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Leagues
            .Include(l => l.Activity)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<League?> GetIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Leagues
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<League> CreateAsync(League league, CancellationToken cancellationToken)
    {
        _context.Leagues.Add(league);
        await _context.SaveChangesAsync(cancellationToken);
        return league;
    }

    public async Task UpdateAsync(Guid id, League league, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}