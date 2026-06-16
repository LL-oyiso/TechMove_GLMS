using GLMS.Api.Mapping;
using GLMS.Api.Models;
using GLMS.Api.Repositories;
using GLMS.Api.Services;
using GLMS.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestRepository _serviceRequests;
    private readonly IContractRepository _contracts;
    private readonly IContractWorkflowService _workflow;
    private readonly ICurrencyConversionService _currency;

    public ServiceRequestsController(
        IServiceRequestRepository serviceRequests,
        IContractRepository contracts,
        IContractWorkflowService workflow,
        ICurrencyConversionService currency)
    {
        _serviceRequests = serviceRequests;
        _contracts = contracts;
        _workflow = workflow;
        _currency = currency;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServiceRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> GetAll(CancellationToken ct)
    {
        var items = await _serviceRequests.GetAllAsync(ct);
        return Ok(items.Select(sr => sr.ToDto()));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequestDto>> GetById(int id, CancellationToken ct)
    {
        var item = await _serviceRequests.GetByIdAsync(id, ct);
        return item is null ? NotFound() : Ok(item.ToDto());
    }

    [HttpGet("estimate")]
    [ProducesResponseType(typeof(CurrencyEstimateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CurrencyEstimateDto>> GetEstimate([FromQuery] decimal usdAmount, CancellationToken ct)
    {
        if (usdAmount <= 0)
        {
            return BadRequest(new { message = "USD amount must be greater than zero." });
        }

        try
        {
            var conversion = await _currency.ConvertUsdToZarAsync(usdAmount, ct);
            return Ok(new CurrencyEstimateDto { CostZar = conversion.ZarAmount, RateUsed = conversion.RateUsed });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "FX service currently unavailable." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ServiceRequestDto>> Create(ServiceRequestInputDto input, CancellationToken ct)
    {
        var contract = await _contracts.GetByIdAsync(input.ContractId, includeClient: false, ct);
        if (contract is null)
        {
            ModelState.AddModelError(nameof(input.ContractId), "Selected contract was not found.");
            return ValidationProblem(ModelState);
        }

        // State pattern: block requests for Expired / On Hold contracts.
        if (!_workflow.CanCreateServiceRequest(contract, out var reason))
        {
            ModelState.AddModelError(nameof(input.ContractId), reason);
            return ValidationProblem(ModelState);
        }

        CurrencyConversionResult conversion;
        try
        {
            conversion = await _currency.ConvertUsdToZarAsync(input.CostUsd, ct);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Currency service is unavailable right now. Please retry." });
        }

        var entity = new ServiceRequest
        {
            ContractId = input.ContractId,
            Description = input.Description,
            CostUsd = input.CostUsd,
            CostZar = conversion.ZarAmount,
            ExchangeRateUsed = conversion.RateUsed,
            Status = input.Status,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _serviceRequests.AddAsync(entity, ct);
        var withContract = await _serviceRequests.GetByIdAsync(created.Id, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, withContract!.ToDto());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Update(int id, ServiceRequestInputDto input, CancellationToken ct)
    {
        var existing = await _serviceRequests.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        var contract = await _contracts.GetByIdAsync(input.ContractId, includeClient: false, ct);
        if (contract is null)
        {
            ModelState.AddModelError(nameof(input.ContractId), "Selected contract was not found.");
            return ValidationProblem(ModelState);
        }

        if (!_workflow.CanCreateServiceRequest(contract, out var reason))
        {
            ModelState.AddModelError(nameof(input.ContractId), reason);
            return ValidationProblem(ModelState);
        }

        CurrencyConversionResult conversion;
        try
        {
            conversion = await _currency.ConvertUsdToZarAsync(input.CostUsd, ct);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Currency service is unavailable right now. Please retry." });
        }

        existing.ContractId = input.ContractId;
        existing.Description = input.Description;
        existing.CostUsd = input.CostUsd;
        existing.CostZar = conversion.ZarAmount;
        existing.ExchangeRateUsed = conversion.RateUsed;
        existing.Status = input.Status;

        await _serviceRequests.UpdateAsync(existing, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _serviceRequests.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        await _serviceRequests.DeleteAsync(existing, ct);
        return NoContent();
    }
}
