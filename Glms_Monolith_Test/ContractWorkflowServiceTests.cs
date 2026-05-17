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
}
