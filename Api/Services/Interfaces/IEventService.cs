using Api.Models;

namespace Api.Services.Interfaces;

public interface IEventService
{
    Task<IEnumerable<ScheduledEvent>> GetAllAsync(CancellationToken cancellationToken);
    Task<IEnumerable<ScheduledEvent>> GetAllByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<ScheduledEvent> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ScheduledEvent> CreateAsync(ScheduledEvent @event, CancellationToken cancellationToken);
    Task UpdateAsync(ScheduledEvent @event, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}