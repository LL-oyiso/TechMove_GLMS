using FluentAssertions;
using GLMS_Monolith.Models.Enums;
using GLMS_Monolith.Services;

namespace Glms_Monolith_Test;

public class ContractObserverTests
{
    [Fact]
    public void NotifyStatusChanged_WithRegisteredObserver_CallsObserver()
    {
        var spy = new SpyObserver();
        var service = new ContractWorkflowService(new[] { spy });

        service.NotifyStatusChanged(1, ContractStatus.Draft, ContractStatus.Active);

        spy.WasNotified.Should().BeTrue();
        spy.LastContractId.Should().Be(1);
        spy.LastPrevious.Should().Be(ContractStatus.Draft);
        spy.LastNew.Should().Be(ContractStatus.Active);
    }

    [Fact]
    public void NotifyStatusChanged_WithMultipleObservers_NotifiesAll()
    {
        var spy1 = new SpyObserver();
        var spy2 = new SpyObserver();
        var service = new ContractWorkflowService(new[] { spy1, spy2 });

        service.NotifyStatusChanged(5, ContractStatus.Active, ContractStatus.OnHold);

        spy1.WasNotified.Should().BeTrue();
        spy2.WasNotified.Should().BeTrue();
    }

    [Fact]
    public void NotifyStatusChanged_WithNoObservers_DoesNotThrow()
    {
        var service = new ContractWorkflowService();

        var act = () => service.NotifyStatusChanged(1, ContractStatus.Draft, ContractStatus.Active);

        act.Should().NotThrow();
    }

    private sealed class SpyObserver : IContractStatusObserver
    {
        public bool WasNotified { get; private set; }
        public int LastContractId { get; private set; }
        public ContractStatus LastPrevious { get; private set; }
        public ContractStatus LastNew { get; private set; }

        public void OnStatusChanged(int contractId, ContractStatus previousStatus, ContractStatus newStatus)
        {
            WasNotified = true;
            LastContractId = contractId;
            LastPrevious = previousStatus;
            LastNew = newStatus;
        }
    }
}
