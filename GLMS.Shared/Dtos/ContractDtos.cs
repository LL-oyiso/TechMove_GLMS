using System.ComponentModel.DataAnnotations;
using GLMS.Shared.Enums;

namespace GLMS.Shared.Dtos;

public class ContractDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ContractStatus Status { get; set; }
    public string ServiceLevel { get; set; } = string.Empty;
    public string? SignedAgreementFileName { get; set; }
    public string? SignedAgreementStoredPath { get; set; }
    public string? SignedAgreementContentType { get; set; }
    public DateTime? SignedAgreementUploadedAt { get; set; }
}

public class ContractInputDto
{
    [Required]
    public int ClientId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required, StringLength(200)]
    public string ServiceLevel { get; set; } = string.Empty;
}

public class ContractStatusUpdateDto
{
    [Required]
    public ContractStatus Status { get; set; }
}
