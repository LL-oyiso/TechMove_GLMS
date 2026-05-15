using GLMS_Monolith.Models;

namespace GLMS_Monolith.Services;

public interface IContractWorkflowService
{
    bool CanCreateServiceRequest(Contract contract, out string reason);
}