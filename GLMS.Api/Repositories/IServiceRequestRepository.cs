using GLMS.Api.Models;

namespace GLMS.Api.Repositories;

public interface IServiceRequestRepository
{
    Task<List<ServiceRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ServiceRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceRequest> AddAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default);
    Task DeleteAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default);
}
