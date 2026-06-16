using GLMS.Shared.Dtos;
using GLMS.Shared.Enums;
using GLMS_Monolith.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS_Monolith.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly IServiceRequestsApi _serviceRequests;
    private readonly IContractsApi _contracts;

    public ServiceRequestsController(IServiceRequestsApi serviceRequests, IContractsApi contracts)
    {
        _serviceRequests = serviceRequests;
        _contracts = contracts;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var items = await _serviceRequests.GetAllAsync(ct);
        return View(items);
    }

    public async Task<IActionResult> Details(int? id, CancellationToken ct)
    {
        if (id is null) return NotFound();
        var item = await _serviceRequests.GetByIdAsync(id.Value, ct);
        return item is null ? NotFound() : View(item);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await PopulateContractDropdownAsync(ct);
        return View(new ServiceRequestInputDto { Status = ServiceRequestStatus.New });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestInputDto input, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateContractDropdownAsync(ct, input.ContractId);
            return View(input);
        }

        try
        {
            await _serviceRequests.CreateAsync(input, ct);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiValidationException ex)
        {
            ModelState.AddApiErrors(ex);
            await PopulateContractDropdownAsync(ct, input.ContractId);
            return View(input);
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateContractDropdownAsync(ct, input.ContractId);
            return View(input);
        }
    }

    public async Task<IActionResult> Edit(int? id, CancellationToken ct)
    {
        if (id is null) return NotFound();
        var item = await _serviceRequests.GetByIdAsync(id.Value, ct);
        if (item is null) return NotFound();

        await PopulateContractDropdownAsync(ct, item.ContractId);
        ViewBag.Id = item.Id;
        return View(new ServiceRequestInputDto
        {
            ContractId = item.ContractId,
            Description = item.Description,
            CostUsd = item.CostUsd,
            Status = item.Status
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceRequestInputDto input, CancellationToken ct)
    {
        ViewBag.Id = id;
        if (!ModelState.IsValid)
        {
            await PopulateContractDropdownAsync(ct, input.ContractId);
            return View(input);
        }

        try
        {
            await _serviceRequests.UpdateAsync(id, input, ct);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiValidationException ex)
        {
            ModelState.AddApiErrors(ex);
            await PopulateContractDropdownAsync(ct, input.ContractId);
            return View(input);
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateContractDropdownAsync(ct, input.ContractId);
            return View(input);
        }
    }

    public async Task<IActionResult> Delete(int? id, CancellationToken ct)
    {
        if (id is null) return NotFound();
        var item = await _serviceRequests.GetByIdAsync(id.Value, ct);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        await _serviceRequests.DeleteAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }

    // Proxies the live FX estimate from the API for the Create page's JavaScript.
    [HttpGet]
    public async Task<IActionResult> GetZarEstimate(decimal usdAmount, CancellationToken ct)
    {
        if (usdAmount <= 0)
        {
            return BadRequest(new { message = "USD amount must be greater than zero." });
        }

        var estimate = await _serviceRequests.GetEstimateAsync(usdAmount, ct);
        if (estimate is null)
        {
            return StatusCode(503, new { message = "FX service currently unavailable." });
        }

        return Json(new { costZar = estimate.CostZar, rateUsed = estimate.RateUsed });
    }

    private async Task PopulateContractDropdownAsync(CancellationToken ct, object? selectedContract = null)
    {
        var contracts = await _contracts.GetAllAsync(null, null, null, ct);
        var options = contracts.Select(c => new
        {
            c.Id,
            Label = $"#{c.Id} - {c.Status} - {c.ClientName ?? "Client"}"
        });

        ViewData["ContractId"] = new SelectList(options, "Id", "Label", selectedContract);
    }
}
