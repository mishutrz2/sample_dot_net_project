using Api.Data;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class EventService(ApplicationDbContext context) : IEventService
{
    public async Task<IEnumerable<ScheduledEvent>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.ScheduledEvents
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduledEvent>> GetAllByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await context.ScheduledEvents
            .Where(e => e.TenantId == tenantId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ScheduledEvent> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<ScheduledEvent> CreateAsync(ScheduledEvent @event, CancellationToken cancellationToken)
    {
        await context.ScheduledEvents.AddAsync(@event, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return @event;
    }

    public Task UpdateAsync(ScheduledEvent @event, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}