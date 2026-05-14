using System.ComponentModel.DataAnnotations;
using GLMS_Monolith.Models.Enums;

namespace GLMS_Monolith.Models;

public class ServiceRequest
{
    public int Id { get; set; }

    [Required]
    public int ContractId { get; set; }

    public Contract? Contract { get; set; }

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 999999999)]
    public decimal CostUsd { get; set; }

    [Range(0, 999999999)]
    public decimal CostZar { get; set; }

    [Range(0, 999999)]
    public decimal ExchangeRateUsed { get; set; }

    [Required]
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.New;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}