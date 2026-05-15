using GLMS_Monolith.Models;
using GLMS_Monolith.Models.Enums;

namespace GLMS_Monolith.Services;

public class ContractWorkflowService : IContractWorkflowService
{
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
}