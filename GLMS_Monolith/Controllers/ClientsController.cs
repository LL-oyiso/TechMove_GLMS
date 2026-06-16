using GLMS.Shared.Dtos;
using GLMS_Monolith.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace GLMS_Monolith.Controllers;

public class ClientsController : Controller
{
    private readonly IClientsApi _api;

    public ClientsController(IClientsApi api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var clients = await _api.GetAllAsync(ct);
        return View(clients);
    }

    public async Task<IActionResult> Details(int? id, CancellationToken ct)
    {
        if (id is null) return NotFound();
        var client = await _api.GetByIdAsync(id.Value, ct);
        return client is null ? NotFound() : View(client);
    }

    public IActionResult Create()
    {
        return View(new ClientInputDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientInputDto input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(input);

        try
        {
            await _api.CreateAsync(input, ct);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiValidationException ex)
        {
            ModelState.AddApiErrors(ex);
            return View(input);
        }
    }

    public async Task<IActionResult> Edit(int? id, CancellationToken ct)
    {
        if (id is null) return NotFound();
        var client = await _api.GetByIdAsync(id.Value, ct);
        if (client is null) return NotFound();

        ViewBag.Id = client.Id;
        return View(new ClientInputDto
        {
            Name = client.Name,
            ContactDetails = client.ContactDetails,
            Region = client.Region
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClientInputDto input, CancellationToken ct)
    {
        ViewBag.Id = id;
        if (!ModelState.IsValid) return View(input);

        try
        {
            await _api.UpdateAsync(id, input, ct);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiValidationException ex)
        {
            ModelState.AddApiErrors(ex);
            return View(input);
        }
    }

    public async Task<IActionResult> Delete(int? id, CancellationToken ct)
    {
        if (id is null) return NotFound();
        var client = await _api.GetByIdAsync(id.Value, ct);
        return client is null ? NotFound() : View(client);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        await _api.DeleteAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }
}
