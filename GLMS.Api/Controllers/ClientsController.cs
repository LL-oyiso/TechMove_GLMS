using GLMS.Api.Mapping;
using GLMS.Api.Repositories;
using GLMS.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientRepository _clients;

    public ClientsController(IClientRepository clients)
    {
        _clients = clients;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll(CancellationToken ct)
    {
        var clients = await _clients.GetAllAsync(ct);
        return Ok(clients.Select(c => c.ToDto()));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetById(int id, CancellationToken ct)
    {
        var client = await _clients.GetByIdAsync(id, ct);
        return client is null ? NotFound() : Ok(client.ToDto());
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> Create(ClientInputDto input, CancellationToken ct)
    {
        var created = await _clients.AddAsync(input.ToEntity(), ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, ClientInputDto input, CancellationToken ct)
    {
        var existing = await _clients.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        existing.Name = input.Name;
        existing.ContactDetails = input.ContactDetails;
        existing.Region = input.Region;

        await _clients.UpdateAsync(existing, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _clients.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        await _clients.DeleteAsync(existing, ct);
        return NoContent();
    }
}
