using GLMS.Shared.Dtos;
using GLMS.Shared.Enums;

namespace GLMS_Monolith.Services.Api;

public record AgreementDownload(Stream Content, string ContentType, string FileName);

public interface IContractsApi
{
    Task<IReadOnlyList<ContractDto>> GetAllAsync(ContractStatus? status, DateTime? startFrom, DateTime? endTo, CancellationToken ct = default);
    Task<ContractDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ContractDto> CreateAsync(ContractInputDto input, CancellationToken ct = default);
    Task UpdateAsync(int id, ContractInputDto input, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task UploadAgreementAsync(int id, Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task<AgreementDownload?> DownloadAgreementAsync(int id, CancellationToken ct = default);
}
