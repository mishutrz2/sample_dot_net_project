using Api.Data;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class LeagueService(ApplicationDbContext _context) : ILeagueService
{
    public async Task<IEnumerable<League>> GetLeaguesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Leagues.Include(l => l.Activity).AsNoTracking().ToListAsync(cancellationToken);
    }

    public Task<League> GetLeagueByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<League> CreateLeagueAsync(League league)
    {
        throw new NotImplementedException();
    }

    public Task UpdateLeagueAsync(int id, League league)
    {
        throw new NotImplementedException();
    }

    public Task DeleteLeagueAsync(int id)
    {
        throw new NotImplementedException();
    }
}