using Api.Models;

namespace Api.Services.Interfaces
{
    public interface ILeagueService
    {
        Task<IEnumerable<League>> GetAsync(CancellationToken cancellationToken);
        Task<League?> GetIdAsync(Guid id, CancellationToken cancellationToken);
        Task<League> CreateAsync(League league, CancellationToken cancellationToken);
        Task UpdateAsync(Guid id, League league, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}