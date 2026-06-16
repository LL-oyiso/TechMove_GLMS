using GLMS.Shared.Enums;

namespace GLMS.Api.Services;

public interface IContractStatusObserver
{
    void OnStatusChanged(int contractId, ContractStatus previousStatus, ContractStatus newStatus);
}
