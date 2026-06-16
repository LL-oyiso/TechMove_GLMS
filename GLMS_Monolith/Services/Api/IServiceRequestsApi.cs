using GLMS.Shared.Dtos;

namespace GLMS_Monolith.Services.Api;

public interface IServiceRequestsApi
{
    Task<IReadOnlyList<ServiceRequestDto>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceRequestDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceRequestDto> CreateAsync(ServiceRequestInputDto input, CancellationToken ct = default);
    Task UpdateAsync(int id, ServiceRequestInputDto input, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<CurrencyEstimateDto?> GetEstimateAsync(decimal usdAmount, CancellationToken ct = default);
}
