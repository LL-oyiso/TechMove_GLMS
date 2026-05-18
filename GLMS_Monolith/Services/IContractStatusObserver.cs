using GLMS_Monolith.Models.Enums;

namespace GLMS_Monolith.Services;

public interface IContractStatusObserver
{
    void OnStatusChanged(int contractId, ContractStatus previousStatus, ContractStatus newStatus);
}
