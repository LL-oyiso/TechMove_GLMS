
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS_Monolith.Models;
using GLMS_Monolith.Data;

public class ContractsController : Controller
{
    private readonly GlmsDbContext _context;

    public ContractsController(GlmsDbContext context)
    {
        _context = context;
    }

    // GET: CONTRACTS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Contracts.ToListAsync());
    }

    // GET: CONTRACTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var contract = await _context.Contracts
            .FirstOrDefaultAsync(m => m.Id == id);
        if (contract == null)
        {
            return NotFound();
        }

        return View(contract);
    }

    // GET: CONTRACTS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: CONTRACTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ClientId,Client,StartDate,EndDate,Status,ServiceLevel,SignedAgreementFileName,SignedAgreementStoredPath,SignedAgreementContentType,SignedAgreementUploadedAt,ServiceRequests")] Contract contract)
    {
        if (ModelState.IsValid)
        {
            _context.Add(contract);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(contract);
    }

    // GET: CONTRACTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null)
        {
            return NotFound();
        }
        return View(contract);
    }

    // POST: CONTRACTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,ClientId,Client,StartDate,EndDate,Status,ServiceLevel,SignedAgreementFileName,SignedAgreementStoredPath,SignedAgreementContentType,SignedAgreementUploadedAt,ServiceRequests")] Contract contract)
    {
        if (id != contract.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(contract);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContractExists(contract.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(contract);
    }

    // GET: CONTRACTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var contract = await _context.Contracts
            .FirstOrDefaultAsync(m => m.Id == id);
        if (contract == null)
        {
            return NotFound();
        }

        return View(contract);
    }

    // POST: CONTRACTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract != null)
        {
            _context.Contracts.Remove(contract);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ContractExists(int? id)
    {
        return _context.Contracts.Any(e => e.Id == id);
    }
}
