
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS_Monolith.Models;
using GLMS_Monolith.Data;

public class ServiceRequestsController : Controller
{
    private readonly GlmsDbContext _context;

    public ServiceRequestsController(GlmsDbContext context)
    {
        _context = context;
    }

    // GET: SERVICEREQUESTS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.ServiceRequests.ToListAsync());
    }

    // GET: SERVICEREQUESTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var servicerequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(m => m.Id == id);
        if (servicerequest == null)
        {
            return NotFound();
        }

        return View(servicerequest);
    }

    // GET: SERVICEREQUESTS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: SERVICEREQUESTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ContractId,Contract,Description,CostUsd,CostZar,ExchangeRateUsed,Status,CreatedAt")] ServiceRequest servicerequest)
    {
        if (ModelState.IsValid)
        {
            _context.Add(servicerequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(servicerequest);
    }

    // GET: SERVICEREQUESTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var servicerequest = await _context.ServiceRequests.FindAsync(id);
        if (servicerequest == null)
        {
            return NotFound();
        }
        return View(servicerequest);
    }

    // POST: SERVICEREQUESTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,ContractId,Contract,Description,CostUsd,CostZar,ExchangeRateUsed,Status,CreatedAt")] ServiceRequest servicerequest)
    {
        if (id != servicerequest.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(servicerequest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceRequestExists(servicerequest.Id))
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
        return View(servicerequest);
    }

    // GET: SERVICEREQUESTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var servicerequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(m => m.Id == id);
        if (servicerequest == null)
        {
            return NotFound();
        }

        return View(servicerequest);
    }

    // POST: SERVICEREQUESTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var servicerequest = await _context.ServiceRequests.FindAsync(id);
        if (servicerequest != null)
        {
            _context.ServiceRequests.Remove(servicerequest);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ServiceRequestExists(int? id)
    {
        return _context.ServiceRequests.Any(e => e.Id == id);
    }
}
