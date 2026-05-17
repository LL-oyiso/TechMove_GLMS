using FluentAssertions;
using GLMS_Monolith.Models;
using GLMS_Monolith.Models.Enums;
using GLMS_Monolith.Services;

namespace Glms_Monolith_Test;

public class ContractWorkflowServiceTests
{
    private readonly ContractWorkflowService _service = new();

    [Fact]
    public void CanCreateServiceRequest_WithExpiredContract_ReturnsFalseAndReason()
    {
        var contract = new Contract { Status = ContractStatus.Expired };

        var canCreate = _service.CanCreateServiceRequest(contract, out var reason);

        canCreate.Should().BeFalse();
        reason.Should().Contain("Expired");
    }

    [Fact]
    public void CanCreateServiceRequest_WithOnHoldContract_ReturnsFalseAndReason()
    {
        var contract = new Contract { Status = ContractStatus.OnHold };

        var canCreate = _service.CanCreateServiceRequest(contract, out var reason);

        canCreate.Should().BeFalse();
        reason.Should().Contain("On Hold");
    }

    [Theory]
    [InlineData(ContractStatus.Draft)]
    [InlineData(ContractStatus.Active)]
    public void CanCreateServiceRequest_WithAllowedStatus_ReturnsTrue(ContractStatus status)
    {
        var contract = new Contract { Status = status };

        var canCreate = _service.CanCreateServiceRequest(contract, out var reason);

        canCreate.Should().BeTrue();
        reason.Should().BeEmpty();
    }

    [Fact]
    public void CanTransitionStatus_FromExpired_ReturnsFalse()
    {
        var allowed = _service.CanTransitionStatus(ContractStatus.Expired, ContractStatus.Active, out var reason);

        allowed.Should().BeFalse();
        reason.Should().Contain("Expired");
    }

    [Theory]
    [InlineData(ContractStatus.Active)]
    [InlineData(ContractStatus.OnHold)]
    public void CanTransitionStatus_ToDraft_ReturnsFalse(ContractStatus status)
    {
        var allowed = _service.CanTransitionStatus(status, ContractStatus.Draft, out var reason);

        allowed.Should().BeFalse();
        reason.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(ContractStatus.Draft, ContractStatus.Active)]
    [InlineData(ContractStatus.Active, ContractStatus.OnHold)]
    [InlineData(ContractStatus.OnHold, ContractStatus.Active)]
    public void CanTransitionStatus_ValidTransition_ReturnsTrue(ContractStatus from, ContractStatus to)
    {
        var allowed = _service.CanTransitionStatus(from, to, out var reason);

        allowed.Should().BeTrue();
        reason.Should().BeEmpty();
    }

    [Theory]
    [InlineData(ContractStatus.Draft)]
    [InlineData(ContractStatus.Active)]
    [InlineData(ContractStatus.OnHold)]
    [InlineData(ContractStatus.Expired)]
    public void CanTransitionStatus_SameStatus_ReturnsTrue(ContractStatus status)
    {
        var allowed = _service.CanTransitionStatus(status, status, out var reason);

        allowed.Should().BeTrue();
        reason.Should().BeEmpty();
    }
}
