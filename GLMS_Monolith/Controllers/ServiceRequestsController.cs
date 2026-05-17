using GLMS_Monolith.Data;
using GLMS_Monolith.Models;
using GLMS_Monolith.Models.Enums;
using GLMS_Monolith.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class ServiceRequestsController : Controller
{
    private readonly GlmsDbContext _context;
    private readonly IContractWorkflowService _workflowService;
    private readonly ICurrencyConversionService _currencyConversionService;

    public ServiceRequestsController(
        GlmsDbContext context,
        IContractWorkflowService workflowService,
        ICurrencyConversionService currencyConversionService)
    {
        _context = context;
        _workflowService = workflowService;
        _currencyConversionService = currencyConversionService;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _context.ServiceRequests
            .Include(sr => sr.Contract!)
            .ThenInclude(c => c.Client)
            .AsNoTracking()
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync();

        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.ServiceRequests
            .Include(sr => sr.Contract!)
            .ThenInclude(c => c.Client)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null) return NotFound();

        return View(item);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateContractDropdownAsync();
        return View(new ServiceRequest { Status = ServiceRequestStatus.New });
    }

    [HttpGet]
    public async Task<IActionResult> GetZarEstimate(decimal usdAmount)
    {
        if (usdAmount <= 0)
        {
            return BadRequest(new { message = "USD amount must be greater than zero." });
        }

        try
        {
            var conversion = await _currencyConversionService.ConvertUsdToZarAsync(usdAmount);
            return Json(new
            {
                costZar = conversion.ZarAmount,
                rateUsed = conversion.RateUsed
            });
        }
        catch (Exception)
        {
            return StatusCode(503, new { message = "FX service currently unavailable." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ContractId,Description,CostUsd,Status")] ServiceRequest input)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == input.ContractId);

        if (contract == null)
        {
            ModelState.AddModelError(nameof(input.ContractId), "Selected contract was not found.");
        }
        else if (!_workflowService.CanCreateServiceRequest(contract, out var reason))
        {
            ModelState.AddModelError(nameof(input.ContractId), reason);
        }

        if (!ModelState.IsValid)
        {
            await PopulateContractDropdownAsync(input.ContractId);
            return View(input);
        }

        try
        {
            var conversion = await _currencyConversionService.ConvertUsdToZarAsync(input.CostUsd);
            input.CostZar = conversion.ZarAmount;
            input.ExchangeRateUsed = conversion.RateUsed;
            input.CreatedAt = DateTime.UtcNow;

            _context.ServiceRequests.Add(input);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Currency service is unavailable right now. Please retry.");
            await PopulateContractDropdownAsync(input.ContractId);
            return View(input);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.ServiceRequests.FindAsync(id);
        if (item == null) return NotFound();

        await PopulateContractDropdownAsync(item.ContractId);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ContractId,Description,CostUsd,Status")] ServiceRequest input)
    {
        if (id != input.Id) return NotFound();

        var existing = await _context.ServiceRequests.FindAsync(id);
        if (existing == null) return NotFound();

        var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == input.ContractId);
        if (contract == null)
        {
            ModelState.AddModelError(nameof(input.ContractId), "Selected contract was not found.");
        }
        else if (!_workflowService.CanCreateServiceRequest(contract, out var reason))
        {
            ModelState.AddModelError(nameof(input.ContractId), reason);
        }

        if (!ModelState.IsValid)
        {
            await PopulateContractDropdownAsync(input.ContractId);
            existing.ContractId = input.ContractId;
            existing.Description = input.Description;
            existing.CostUsd = input.CostUsd;
            existing.Status = input.Status;
            return View(existing);
        }

        try
        {
            var conversion = await _currencyConversionService.ConvertUsdToZarAsync(input.CostUsd);

            existing.ContractId = input.ContractId;
            existing.Description = input.Description;
            existing.CostUsd = input.CostUsd;
            existing.CostZar = conversion.ZarAmount;
            existing.ExchangeRateUsed = conversion.RateUsed;
            existing.Status = input.Status;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Currency service is unavailable right now. Please retry.");
            await PopulateContractDropdownAsync(input.ContractId);
            existing.ContractId = input.ContractId;
            existing.Description = input.Description;
            existing.CostUsd = input.CostUsd;
            existing.Status = input.Status;
            return View(existing);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.ServiceRequests
            .Include(sr => sr.Contract!)
            .ThenInclude(c => c.Client)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null) return NotFound();

        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.ServiceRequests.FindAsync(id);
        if (item != null)
        {
            _context.ServiceRequests.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateContractDropdownAsync(object? selectedContract = null)
    {
        var contracts = await _context.Contracts
            .Include(c => c.Client)
            .AsNoTracking()
            .OrderByDescending(c => c.Id)
            .Select(c => new
            {
                c.Id,
                Label = $"#{c.Id} - {c.Status} - {(c.Client != null ? c.Client.Name : "Client")}"
            })
            .ToListAsync();

        ViewData["ContractId"] = new SelectList(contracts, "Id", "Label", selectedContract);
    }
}