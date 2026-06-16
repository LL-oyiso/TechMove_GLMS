using GLMS.Shared.Dtos;

namespace GLMS_Monolith.Services.Api;

public interface IClientsApi
{
    Task<IReadOnlyList<ClientDto>> GetAllAsync(CancellationToken ct = default);
    Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClientDto> CreateAsync(ClientInputDto input, CancellationToken ct = default);
    Task UpdateAsync(int id, ClientInputDto input, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
