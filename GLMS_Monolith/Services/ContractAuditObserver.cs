using GLMS_Monolith.Models.Enums;
using Microsoft.Extensions.Logging;

namespace GLMS_Monolith.Services;

public class ContractAuditObserver : IContractStatusObserver
{
    private readonly ILogger<ContractAuditObserver> _logger;

    public ContractAuditObserver(ILogger<ContractAuditObserver> logger)
    {
        _logger = logger;
    }

    public void OnStatusChanged(int contractId, ContractStatus previousStatus, ContractStatus newStatus)
    {
        _logger.LogInformation(
            "[GLMS Audit] Contract #{ContractId} status changed: {Previous} → {New} at {Timestamp} (UTC)",
            contractId, previousStatus, newStatus, DateTime.UtcNow);
    }
}
