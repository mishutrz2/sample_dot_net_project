using Api.Models;

namespace Api.Services.Interfaces
{
    public interface ITenantService
    {
        Task<IEnumerable<Tenant>> GetAsync(CancellationToken cancellationToken);
        Task<Tenant?> GetIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken);
        Task UpdateAsync(Guid id, Tenant tenant, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}