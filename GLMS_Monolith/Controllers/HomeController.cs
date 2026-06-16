using System.Diagnostics;
using GLMS.Shared.Enums;
using GLMS_Monolith.Models;
using GLMS_Monolith.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace GLMS_Monolith.Controllers;

public class HomeController : Controller
{
    private readonly IContractsApi _contracts;
    private readonly IServiceRequestsApi _serviceRequests;

    public HomeController(IContractsApi contracts, IServiceRequestsApi serviceRequests)
    {
        _contracts = contracts;
        _serviceRequests = serviceRequests;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var contracts = await _contracts.GetAllAsync(null, null, null, ct);
        var requests = await _serviceRequests.GetAllAsync(ct);

        ViewBag.ActiveContracts = contracts.Count(c => c.Status == ContractStatus.Active);
        ViewBag.OnHoldContracts = contracts.Count(c => c.Status == ContractStatus.OnHold);
        ViewBag.RequestsToday = requests.Count(r => r.CreatedAt.Date == DateTime.UtcNow.Date);
        ViewBag.TotalRequests = requests.Count;

        ViewBag.AttentionContracts = contracts
            .Where(c => c.Status == ContractStatus.Expired || c.Status == ContractStatus.OnHold)
            .OrderByDescending(c => c.Id)
            .Take(5)
            .ToList();

        ViewBag.RecentRequests = requests
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToList();

        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
