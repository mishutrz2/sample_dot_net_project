using Api.Data;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class TenantService(ApplicationDbContext _context) : ITenantService
{
    public async Task<IEnumerable<Tenant>> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Include(t => t.Activity)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Tenant?> GetIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task UpdateAsync(Guid id, Tenant tenant, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}