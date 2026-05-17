using FluentAssertions;
using GLMS_Monolith.Data;
using GLMS_Monolith.Models;
using GLMS_Monolith.Models.Enums;
using GLMS_Monolith.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glms_Monolith_Test;

public class ServiceRequestsControllerTests
{
    [Fact]
    public async Task Create_WithValidInput_SetsConvertedFieldsAndRedirects()
    {
        await using var context = BuildContext();
        await SeedContractAsync(context, ContractStatus.Active);

        var controller = new global::ServiceRequestsController(
            context,
            new ContractWorkflowService(),
            new StubCurrencyConversionService(rate: 18.5m, zar: 1850m)
        );

        var input = new ServiceRequest
        {
            ContractId = 1,
            Description = "Move cargo",
            CostUsd = 100m,
            Status = ServiceRequestStatus.New
        };

        var result = await controller.Create(input);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(global::ServiceRequestsController.Index));

        var saved = await context.ServiceRequests.SingleAsync();
        saved.CostUsd.Should().Be(100m);
        saved.CostZar.Should().Be(1850m);
        saved.ExchangeRateUsed.Should().Be(18.5m);
        saved.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetZarEstimate_WhenConversionFails_Returns503()
    {
        await using var context = BuildContext();
        var controller = new global::ServiceRequestsController(
            context,
            new ContractWorkflowService(),
            new ThrowingCurrencyConversionService()
        );

        var result = await controller.GetZarEstimate(150m);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(503);
    }

    [Fact]
    public async Task GetZarEstimate_WithZeroAmount_ReturnsBadRequest()
    {
        await using var context = BuildContext();
        var controller = new global::ServiceRequestsController(
            context,
            new ContractWorkflowService(),
            new StubCurrencyConversionService(rate: 18.5m, zar: 0m)
        );

        var result = await controller.GetZarEstimate(0m);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    private static GlmsDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<GlmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new GlmsDbContext(options);
    }

    private static async Task SeedContractAsync(GlmsDbContext context, ContractStatus status)
    {
        var client = new Client
        {
            Id = 1,
            Name = "Demo Client",
            ContactDetails = "demo@client.com",
            Region = "Gauteng"
        };

        var contract = new Contract
        {
            Id = 1,
            ClientId = client.Id,
            Client = client,
            Status = status,
            ServiceLevel = "Gold",
            StartDate = DateTime.UtcNow.Date.AddDays(-7),
            EndDate = DateTime.UtcNow.Date.AddDays(30)
        };

        context.Clients.Add(client);
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();
    }

    private sealed class StubCurrencyConversionService(decimal rate, decimal zar) : ICurrencyConversionService
    {
        public Task<CurrencyConversionResult> ConvertUsdToZarAsync(decimal usdAmount, CancellationToken cancellationToken = default)
            => Task.FromResult(new CurrencyConversionResult(usdAmount, rate, zar));
    }

    private sealed class ThrowingCurrencyConversionService : ICurrencyConversionService
    {
        public Task<CurrencyConversionResult> ConvertUsdToZarAsync(decimal usdAmount, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("fx down");
    }
}
