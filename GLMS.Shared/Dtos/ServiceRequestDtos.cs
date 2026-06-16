using System.ComponentModel.DataAnnotations;
using GLMS.Shared.Enums;

namespace GLMS.Shared.Dtos;

public class ServiceRequestDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string? ClientName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal CostUsd { get; set; }
    public decimal CostZar { get; set; }
    public decimal ExchangeRateUsed { get; set; }
    public ServiceRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ServiceRequestInputDto
{
    [Required]
    public int ContractId { get; set; }

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 999999999)]
    public decimal CostUsd { get; set; }

    [Required]
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.New;
}

public class CurrencyEstimateDto
{
    public decimal CostZar { get; set; }
    public decimal RateUsed { get; set; }
}
