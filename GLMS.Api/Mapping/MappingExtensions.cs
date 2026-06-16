using GLMS.Api.Models;
using GLMS.Shared.Dtos;

namespace GLMS.Api.Mapping;

public static class MappingExtensions
{
    public static ClientDto ToDto(this Client c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        ContactDetails = c.ContactDetails,
        Region = c.Region
    };

    public static Client ToEntity(this ClientInputDto dto) => new()
    {
        Name = dto.Name,
        ContactDetails = dto.ContactDetails,
        Region = dto.Region
    };

    public static ContractDto ToDto(this Contract c) => new()
    {
        Id = c.Id,
        ClientId = c.ClientId,
        ClientName = c.Client?.Name,
        StartDate = c.StartDate,
        EndDate = c.EndDate,
        Status = c.Status,
        ServiceLevel = c.ServiceLevel,
        SignedAgreementFileName = c.SignedAgreementFileName,
        SignedAgreementStoredPath = c.SignedAgreementStoredPath,
        SignedAgreementContentType = c.SignedAgreementContentType,
        SignedAgreementUploadedAt = c.SignedAgreementUploadedAt
    };

    public static ServiceRequestDto ToDto(this ServiceRequest sr) => new()
    {
        Id = sr.Id,
        ContractId = sr.ContractId,
        ClientName = sr.Contract?.Client?.Name,
        Description = sr.Description,
        CostUsd = sr.CostUsd,
        CostZar = sr.CostZar,
        ExchangeRateUsed = sr.ExchangeRateUsed,
        Status = sr.Status,
        CreatedAt = sr.CreatedAt
    };
}
