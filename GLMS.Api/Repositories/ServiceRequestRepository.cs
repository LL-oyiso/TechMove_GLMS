using GLMS.Api.Data;
using GLMS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Repositories;

public class ServiceRequestRepository : IServiceRequestRepository
{
    private readonly GlmsDbContext _context;

    public ServiceRequestRepository(GlmsDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceRequest>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.ServiceRequests
            .Include(sr => sr.Contract!)
                .ThenInclude(c => c.Client)
            .AsNoTracking()
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<ServiceRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.ServiceRequests
            .Include(sr => sr.Contract!)
                .ThenInclude(c => c.Client)
            .FirstOrDefaultAsync(sr => sr.Id == id, cancellationToken);

    public async Task<ServiceRequest> AddAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync(cancellationToken);
        return serviceRequest;
    }

    public async Task UpdateAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        _context.ServiceRequests.Update(serviceRequest);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        _context.ServiceRequests.Remove(serviceRequest);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
