using GLMS.Api.Models;
using GLMS.Shared.Enums;

namespace GLMS.Api.Services;

public interface IContractWorkflowService
{
    bool CanCreateServiceRequest(Contract contract, out string reason);
    bool CanTransitionStatus(ContractStatus currentStatus, ContractStatus newStatus, out string reason);
    void NotifyStatusChanged(int contractId, ContractStatus previousStatus, ContractStatus newStatus);
}
