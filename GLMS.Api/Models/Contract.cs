using System.ComponentModel.DataAnnotations;
using GLMS.Shared.Enums;

namespace GLMS.Api.Models;

public class Contract
{
    public int Id { get; set; }

    [Required]
    public int ClientId { get; set; }

    public Client? Client { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required, StringLength(200)]
    public string ServiceLevel { get; set; } = string.Empty;

    // PDF metadata (signed agreement)
    [StringLength(255)]
    public string? SignedAgreementFileName { get; set; }

    [StringLength(500)]
    public string? SignedAgreementStoredPath { get; set; }

    [StringLength(100)]
    public string? SignedAgreementContentType { get; set; }

    public DateTime? SignedAgreementUploadedAt { get; set; }

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
