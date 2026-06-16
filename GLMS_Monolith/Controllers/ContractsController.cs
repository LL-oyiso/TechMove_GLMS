using GLMS.Shared;
using GLMS.Shared.Dtos;
using GLMS.Shared.Enums;
using GLMS_Monolith.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS_Monolith.Controllers;

public class ContractsController : Controller
{
    private readonly IContractsApi _contracts;
    private readonly IClientsApi _clients;

    public ContractsController(IContractsApi contracts, IClientsApi clients)
    {
        _contracts = contracts;
        _clients = clients;
    }

    public async Task<IActionResult> Index(string? status, DateTime? startFrom, DateTime? endTo, CancellationToken ct)
    {
        ContractStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ContractStatus>(status, out var s))
        {
            parsedStatus = s;
        }

        var contracts = await _contracts.GetAllAsync(parsedStatus, startFrom, endTo, ct);

        ViewBag.CurrentStatus = status;
        ViewBag.StartFrom = startFrom?.ToString("yyyy-MM-dd");
        ViewBag.EndTo = endTo?.ToString("yyyy-MM-dd");
        ViewBag.StatusOptions = Enum.GetNames(typeof(ContractStatus));

        return View(contracts);
    }

    public async Task<IActionResult> Details(int? id, CancellationToken ct)
    {
        if (id is null) return NotFound();
        var contract = await _contracts.GetByIdAsync(id.Value, ct);
        return contract is null ? NotFound() : View(contract);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await PopulateDropdownsAsync(ct);
        return View(new ContractInputDto
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1),
            Status = ContractStatus.Draft
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractInputDto input, IFormFile? signedAgreementFile, CancellationToken ct)
    {
        if (signedAgreementFile is null || signedAgreementFile.Length == 0)
        {
            ModelState.AddModelError("signedAgreementFile", "A signed agreement PDF is required.");
        }
        else if (!Path.GetExtension(signedAgreementFile.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("signedAgreementFile", "Only .pdf files are allowed.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(ct, input.ClientId, input.ServiceLevel);
            return View(input);
        }

        try
        {
            var created = await _contracts.CreateAsync(input, ct);
            await UploadAsync(created.Id, signedAgreementFile!, ct);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiValidationException ex)
        {
            ModelState.AddApiErrors(ex);
            await PopulateDropdownsAsync(ct, input.ClientId, input.ServiceLevel);
            return View(input);
        }
    }

    public async Task<IActionResult> Edit(int? id, CancellationToken ct)
    {
        if (id is null) return NotFound();
        var contract = await _contracts.GetByIdAsync(id.Value, ct);
        if (contract is null) return NotFound();

        await PopulateDropdownsAsync(ct, contract.ClientId, contract.ServiceLevel);
        ViewBag.Id = contract.Id;
        ViewBag.CurrentFileName = contract.SignedAgreementFileName;
        ViewBag.HasFile = !string.IsNullOrWhiteSpace(contract.SignedAgreementStoredPath);

        return View(new ContractInputDto
        {
            ClientId = contract.ClientId,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status,
            ServiceLevel = contract.ServiceLevel
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContractInputDto input, IFormFile? signedAgreementFile, CancellationToken ct)
    {
        if (signedAgreementFile is not null && signedAgreementFile.Length > 0 &&
            !Path.GetExtension(signedAgreementFile.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("signedAgreementFile", "Only .pdf files are allowed.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(ct, input.ClientId, input.ServiceLevel);
            ViewBag.Id = id;
            return View(input);
        }

        try
        {
            await _contracts.UpdateAsync(id, input, ct);

            if (signedAgreementFile is not null && signedAgreementFile.Length > 0)
            {
                await UploadAsync(id, signedAgreementFile, ct);
            }

            return RedirectToAction(nameof(Index));
        }
        catch (ApiValidationException ex)
        {
            ModelState.AddApiErrors(ex);
            await PopulateDropdownsAsync(ct, input.ClientId, input.ServiceLevel);
            ViewBag.Id = id;
            return View(input);
        }
    }

    public async Task<IActionResult> Delete(int? id, CancellationToken ct)
    {
        if (id is null) return NotFound();
        var contract = await _contracts.GetByIdAsync(id.Value, ct);
        return contract is null ? NotFound() : View(contract);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        await _contracts.DeleteAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DownloadAgreement(int id, CancellationToken ct)
    {
        var download = await _contracts.DownloadAgreementAsync(id, ct);
        if (download is null) return NotFound("Agreement file not found.");
        return File(download.Content, download.ContentType, download.FileName);
    }

    private async Task UploadAsync(int contractId, IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        await _contracts.UploadAgreementAsync(contractId, stream, file.FileName, file.ContentType, ct);
    }

    private async Task PopulateDropdownsAsync(CancellationToken ct, object? selectedClient = null, string? selectedLevel = null)
    {
        var clients = await _clients.GetAllAsync(ct);
        ViewBag.ClientId = new SelectList(clients, "Id", "Name", selectedClient);
        ViewBag.ServiceLevels = new SelectList(ServiceLevels.All, selectedLevel);
    }
}
