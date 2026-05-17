using GLMS_Monolith.Data;
using GLMS_Monolith.Models;
using GLMS_Monolith.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class ContractsController : Controller
{
    private readonly GlmsDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IContractWorkflowService _workflowService;

    public ContractsController(GlmsDbContext context, IFileStorageService fileStorage, IContractWorkflowService workflowService)
    {
        _context = context;
        _fileStorage = fileStorage;
        _workflowService = workflowService;
    }

    private static readonly string[] AllowedServiceLevels =
{
    "Bronze",
    "Silver",
    "Gold",
    "Platinum",
    "Enterprise"
};

    private void PopulateServiceLevelOptions(string? selected = null)
    {
        ViewBag.ServiceLevels = new SelectList(AllowedServiceLevels, selected);
    }

    private bool IsValidServiceLevel(string? level)
    {
        return !string.IsNullOrWhiteSpace(level)
            && AllowedServiceLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
    }



    // GET: Contracts
    public async Task<IActionResult> Index(string? status, DateTime? startFrom, DateTime? endTo)
    {
        var query = _context.Contracts
            .Include(c => c.Client)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<GLMS_Monolith.Models.Enums.ContractStatus>(status, out var parsedStatus))
        {
            query = query.Where(c => c.Status == parsedStatus);
        }

        if (startFrom.HasValue)
        {
            query = query.Where(c => c.StartDate >= startFrom.Value.Date);
        }

        if (endTo.HasValue)
        {
            query = query.Where(c => c.EndDate <= endTo.Value.Date);
        }

        var contracts = await query
            .OrderByDescending(c => c.Id)
            .ToListAsync();

        ViewBag.CurrentStatus = status;
        ViewBag.StartFrom = startFrom?.ToString("yyyy-MM-dd");
        ViewBag.EndTo = endTo?.ToString("yyyy-MM-dd");
        ViewBag.StatusOptions = Enum.GetNames(typeof(GLMS_Monolith.Models.Enums.ContractStatus));

        return View(contracts);
    }

    // GET: Contracts/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var contract = await _context.Contracts
            .Include(c => c.Client)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (contract == null) return NotFound();

        return View(contract);
    }

    // GET: Contracts/Create
    public async Task<IActionResult> Create()
    {
        await PopulateClientDropdownAsync();
        PopulateServiceLevelOptions();

        return View(new Contract
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1)
        });
    }

    // POST: Contracts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract,
        IFormFile? signedAgreementFile)
    {
        if (contract.EndDate < contract.StartDate)
        {
            ModelState.AddModelError(nameof(contract.EndDate), "End date cannot be before start date.");
        }

        if (signedAgreementFile == null || signedAgreementFile.Length == 0)
        {
            ModelState.AddModelError("signedAgreementFile", "Signed Agreement PDF is required.");
        }
        else if (!_fileStorage.IsPdf(signedAgreementFile, out var error))
        {
            ModelState.AddModelError("signedAgreementFile", error);
        }

        if (!IsValidServiceLevel(contract.ServiceLevel))
        {
            ModelState.AddModelError(nameof(contract.ServiceLevel), "Please choose a valid service level.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateClientDropdownAsync(contract.ClientId);
            PopulateServiceLevelOptions(contract.ServiceLevel);
            return View(contract);
        }

        var stored = await _fileStorage.SaveContractAgreementAsync(signedAgreementFile!);
        contract.SignedAgreementFileName = stored.OriginalFileName;
        contract.SignedAgreementStoredPath = stored.StoredRelativePath;
        contract.SignedAgreementContentType = stored.ContentType;
        contract.SignedAgreementUploadedAt = stored.UploadedAtUtc;

        _context.Add(contract);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Contracts/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return NotFound();

        await PopulateClientDropdownAsync(contract.ClientId);
        PopulateServiceLevelOptions(contract.ServiceLevel);
        return View(contract);
    }

    // POST: Contracts/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract input,
        IFormFile? signedAgreementFile,
        bool removeExistingFile = false)
    {
        if (id != input.Id) return NotFound();

        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return NotFound();

        if (input.EndDate < input.StartDate)
        {
            ModelState.AddModelError(nameof(input.EndDate), "End date cannot be before start date.");
        }

        if (!_workflowService.CanTransitionStatus(contract.Status, input.Status, out var transitionReason))
        {
            ModelState.AddModelError(nameof(input.Status), transitionReason);
        }

        if (signedAgreementFile != null && !_fileStorage.IsPdf(signedAgreementFile, out var error))
        {
            ModelState.AddModelError("signedAgreementFile", error);
        }

        if (!IsValidServiceLevel(input.ServiceLevel))
        {
            ModelState.AddModelError(nameof(input.ServiceLevel), "Please choose a valid service level.");
        }

        if (!ModelState.IsValid)
        {
            contract.ClientId = input.ClientId;
            contract.StartDate = input.StartDate;
            contract.EndDate = input.EndDate;
            contract.Status = input.Status;
            contract.ServiceLevel = input.ServiceLevel;

            await PopulateClientDropdownAsync(input.ClientId);
            PopulateServiceLevelOptions(input.ServiceLevel);
            return View(contract);
        }

        contract.ClientId = input.ClientId;
        contract.StartDate = input.StartDate;
        contract.EndDate = input.EndDate;
        contract.Status = input.Status;
        contract.ServiceLevel = input.ServiceLevel;

        if (removeExistingFile)
        {
            _fileStorage.DeleteIfExists(contract.SignedAgreementStoredPath);
            contract.SignedAgreementFileName = null;
            contract.SignedAgreementStoredPath = null;
            contract.SignedAgreementContentType = null;
            contract.SignedAgreementUploadedAt = null;
        }

        if (signedAgreementFile != null && signedAgreementFile.Length > 0)
        {
            _fileStorage.DeleteIfExists(contract.SignedAgreementStoredPath);

            var stored = await _fileStorage.SaveContractAgreementAsync(signedAgreementFile);
            contract.SignedAgreementFileName = stored.OriginalFileName;
            contract.SignedAgreementStoredPath = stored.StoredRelativePath;
            contract.SignedAgreementContentType = stored.ContentType;
            contract.SignedAgreementUploadedAt = stored.UploadedAtUtc;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Contracts/DownloadAgreement/5
    [HttpGet]
    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var contract = await _context.Contracts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (contract == null || string.IsNullOrWhiteSpace(contract.SignedAgreementStoredPath))
        {
            return NotFound("Agreement file not found.");
        }

        var fullPath = _fileStorage.GetFullPath(contract.SignedAgreementStoredPath);
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound("File missing on server.");
        }

        var downloadName = string.IsNullOrWhiteSpace(contract.SignedAgreementFileName)
            ? $"contract-{contract.Id}.pdf"
            : contract.SignedAgreementFileName;

        return PhysicalFile(fullPath, "application/pdf", downloadName);
    }

    // GET: Contracts/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var contract = await _context.Contracts
            .Include(c => c.Client)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (contract == null) return NotFound();

        return View(contract);
    }

    // POST: Contracts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract != null)
        {
            _fileStorage.DeleteIfExists(contract.SignedAgreementStoredPath);
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateClientDropdownAsync(object? selectedClient = null)
    {
        var clients = await _context.Clients
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewData["ClientId"] = new SelectList(clients, "Id", "Name", selectedClient);
    }
}