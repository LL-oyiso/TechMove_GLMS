using GLMS.Api.Models;
using GLMS.Shared.Enums;

namespace GLMS.Api.Repositories;

public interface IContractRepository
{
    Task<List<Contract>> GetAllAsync(ContractStatus? status, DateTime? startFrom, DateTime? endTo, CancellationToken cancellationToken = default);
    Task<Contract?> GetByIdAsync(int id, bool includeClient = true, CancellationToken cancellationToken = default);
    Task<Contract> AddAsync(Contract contract, CancellationToken cancellationToken = default);
    Task UpdateAsync(Contract contract, CancellationToken cancellationToken = default);
    Task DeleteAsync(Contract contract, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
