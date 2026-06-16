using GLMS.Api.Mapping;
using GLMS.Api.Models;
using GLMS.Api.Repositories;
using GLMS.Api.Services;
using GLMS.Shared;
using GLMS.Shared.Dtos;
using GLMS.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractRepository _contracts;
    private readonly IClientRepository _clients;
    private readonly IContractWorkflowService _workflow;
    private readonly IFileStorageService _files;

    public ContractsController(
        IContractRepository contracts,
        IClientRepository clients,
        IContractWorkflowService workflow,
        IFileStorageService files)
    {
        _contracts = contracts;
        _clients = clients;
        _workflow = workflow;
        _files = files;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContractDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ContractDto>>> GetAll(
        [FromQuery] ContractStatus? status,
        [FromQuery] DateTime? startFrom,
        [FromQuery] DateTime? endTo,
        CancellationToken ct)
    {
        var contracts = await _contracts.GetAllAsync(status, startFrom, endTo, ct);
        return Ok(contracts.Select(c => c.ToDto()));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractDto>> GetById(int id, CancellationToken ct)
    {
        var contract = await _contracts.GetByIdAsync(id, includeClient: true, ct);
        return contract is null ? NotFound() : Ok(contract.ToDto());
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContractDto>> Create(ContractInputDto input, CancellationToken ct)
    {
        if (!await ValidateInputAsync(input, ct)) return ValidationProblem(ModelState);

        var contract = new Contract
        {
            ClientId = input.ClientId,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            Status = input.Status,
            ServiceLevel = input.ServiceLevel
        };

        var created = await _contracts.AddAsync(contract, ct);
        var withClient = await _contracts.GetByIdAsync(created.Id, includeClient: true, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, withClient!.ToDto());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, ContractInputDto input, CancellationToken ct)
    {
        var existing = await _contracts.GetByIdAsync(id, includeClient: false, ct);
        if (existing is null) return NotFound();

        if (!await ValidateInputAsync(input, ct)) return ValidationProblem(ModelState);

        // State pattern: only allow legal status transitions.
        if (!_workflow.CanTransitionStatus(existing.Status, input.Status, out var reason))
        {
            ModelState.AddModelError(nameof(input.Status), reason);
            return ValidationProblem(ModelState);
        }

        var previousStatus = existing.Status;

        existing.ClientId = input.ClientId;
        existing.StartDate = input.StartDate;
        existing.EndDate = input.EndDate;
        existing.Status = input.Status;
        existing.ServiceLevel = input.ServiceLevel;

        await _contracts.UpdateAsync(existing, ct);

        // Observer pattern: notify listeners when the status actually changed.
        if (previousStatus != existing.Status)
        {
            _workflow.NotifyStatusChanged(existing.Id, previousStatus, existing.Status);
        }

        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, ContractStatusUpdateDto input, CancellationToken ct)
    {
        var existing = await _contracts.GetByIdAsync(id, includeClient: false, ct);
        if (existing is null) return NotFound();

        if (!_workflow.CanTransitionStatus(existing.Status, input.Status, out var reason))
        {
            ModelState.AddModelError(nameof(input.Status), reason);
            return ValidationProblem(ModelState);
        }

        var previousStatus = existing.Status;
        existing.Status = input.Status;
        await _contracts.UpdateAsync(existing, ct);

        if (previousStatus != existing.Status)
        {
            _workflow.NotifyStatusChanged(existing.Id, previousStatus, existing.Status);
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _contracts.GetByIdAsync(id, includeClient: false, ct);
        if (existing is null) return NotFound();

        _files.DeleteIfExists(existing.SignedAgreementStoredPath);
        await _contracts.DeleteAsync(existing, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/agreement")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractDto>> UploadAgreement(int id, IFormFile file, CancellationToken ct)
    {
        var existing = await _contracts.GetByIdAsync(id, includeClient: false, ct);
        if (existing is null) return NotFound();

        if (file is null)
        {
            return BadRequest(new { message = "A PDF file is required." });
        }

        if (!_files.IsPdf(file, out var validationError))
        {
            return BadRequest(new { message = validationError });
        }

        // Replace any previous file.
        _files.DeleteIfExists(existing.SignedAgreementStoredPath);

        var stored = await _files.SaveContractAgreementAsync(file, ct);
        existing.SignedAgreementFileName = stored.OriginalFileName;
        existing.SignedAgreementStoredPath = stored.StoredRelativePath;
        existing.SignedAgreementContentType = stored.ContentType;
        existing.SignedAgreementUploadedAt = stored.UploadedAtUtc;

        await _contracts.UpdateAsync(existing, ct);

        var withClient = await _contracts.GetByIdAsync(existing.Id, includeClient: true, ct);
        return Ok(withClient!.ToDto());
    }

    [HttpGet("{id:int}/agreement")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAgreement(int id, CancellationToken ct)
    {
        var contract = await _contracts.GetByIdAsync(id, includeClient: false, ct);
        if (contract is null || string.IsNullOrWhiteSpace(contract.SignedAgreementStoredPath))
        {
            return NotFound("Agreement file not found.");
        }

        var fullPath = _files.GetFullPath(contract.SignedAgreementStoredPath);
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound("File missing on server.");
        }

        var downloadName = string.IsNullOrWhiteSpace(contract.SignedAgreementFileName)
            ? $"contract-{contract.Id}.pdf"
            : contract.SignedAgreementFileName;

        return PhysicalFile(fullPath, "application/pdf", downloadName);
    }

    private async Task<bool> ValidateInputAsync(ContractInputDto input, CancellationToken ct)
    {
        if (input.EndDate < input.StartDate)
        {
            ModelState.AddModelError(nameof(input.EndDate), "End date cannot be before start date.");
        }

        if (!ServiceLevels.IsValid(input.ServiceLevel))
        {
            ModelState.AddModelError(nameof(input.ServiceLevel), "Please choose a valid service level.");
        }

        if (!await _clients.ExistsAsync(input.ClientId, ct))
        {
            ModelState.AddModelError(nameof(input.ClientId), "Selected client was not found.");
        }

        return ModelState.IsValid;
    }
}
