using GLMS_Monolith.Models;
using GLMS_Monolith.Models.Enums;

namespace GLMS_Monolith.Services;

public class ContractWorkflowService : IContractWorkflowService
{
    private readonly IReadOnlyList<IContractStatusObserver> _observers;

    public ContractWorkflowService(IEnumerable<IContractStatusObserver>? observers = null)
    {
        _observers = observers?.ToList() ?? new List<IContractStatusObserver>();
    }

    public bool CanCreateServiceRequest(Contract contract, out string reason)
    {
        reason = string.Empty;

        if (contract.Status == ContractStatus.Expired)
        {
            reason = "Cannot create a service request for an Expired contract.";
            return false;
        }

        if (contract.Status == ContractStatus.OnHold)
        {
            reason = "Cannot create a service request for a contract that is On Hold.";
            return false;
        }

        return true;
    }

    public bool CanTransitionStatus(ContractStatus currentStatus, ContractStatus newStatus, out string reason)
    {
        reason = string.Empty;

        if (currentStatus == newStatus)
            return true;

        if (currentStatus == ContractStatus.Expired)
        {
            reason = "An Expired contract cannot change status.";
            return false;
        }

        var allowed = currentStatus switch
        {
            ContractStatus.Draft => new[] { ContractStatus.Active, ContractStatus.OnHold },
            ContractStatus.Active => new[] { ContractStatus.OnHold, ContractStatus.Expired },
            ContractStatus.OnHold => new[] { ContractStatus.Active, ContractStatus.Expired },
            _ => Array.Empty<ContractStatus>()
        };

        if (!allowed.Contains(newStatus))
        {
            reason = $"Cannot transition a contract from {currentStatus} to {newStatus}.";
            return false;
        }

        return true;
    }

    public void NotifyStatusChanged(int contractId, ContractStatus previousStatus, ContractStatus newStatus)
    {
        foreach (var observer in _observers)
        {
            observer.OnStatusChanged(contractId, previousStatus, newStatus);
        }
    }
}
