using GLMS_Monolith.Data;
using GLMS_Monolith.Models;
using GLMS_Monolith.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GLMS_Monolith.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GlmsDbContext _context;

        public HomeController(ILogger<HomeController> logger, GlmsDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ActiveContracts = await _context.Contracts
                .CountAsync(c => c.Status == ContractStatus.Active);

            ViewBag.OnHoldContracts = await _context.Contracts
                .CountAsync(c => c.Status == ContractStatus.OnHold);

            ViewBag.RequestsToday = await _context.ServiceRequests
                .CountAsync(r => r.CreatedAt.Date == DateTime.UtcNow.Date);

            ViewBag.TotalRequests = await _context.ServiceRequests.CountAsync();

            ViewBag.AttentionContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Expired || c.Status == ContractStatus.OnHold)
                .OrderByDescending(c => c.Id)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentRequests = await _context.ServiceRequests
                .Include(sr => sr.Contract!)
                    .ThenInclude(c => c.Client)
                .OrderByDescending(sr => sr.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
