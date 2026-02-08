using Api.Models;

namespace Api.Services.Interfaces
{
    public interface ILeagueService
    {
        Task<IEnumerable<League>> GetLeaguesAsync(CancellationToken cancellationToken = default);
        Task<League> GetLeagueByIdAsync(int id);
        Task<League> CreateLeagueAsync(League league);
        Task UpdateLeagueAsync(int id, League league);
        Task DeleteLeagueAsync(int id);
    }
}