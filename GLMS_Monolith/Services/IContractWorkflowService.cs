using GLMS_Monolith.Models;
using GLMS_Monolith.Models.Enums;

namespace GLMS_Monolith.Services;

public interface IContractWorkflowService
{
    bool CanCreateServiceRequest(Contract contract, out string reason);
    bool CanTransitionStatus(ContractStatus currentStatus, ContractStatus newStatus, out string reason);
}