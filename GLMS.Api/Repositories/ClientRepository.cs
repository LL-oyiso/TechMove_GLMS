using GLMS.Api.Data;
using GLMS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly GlmsDbContext _context;

    public ClientRepository(GlmsDbContext context)
    {
        _context = context;
    }

    public async Task<List<Client>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Clients.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public async Task<Client?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync(cancellationToken);
        return client;
    }

    public async Task UpdateAsync(Client client, CancellationToken cancellationToken = default)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Client client, CancellationToken cancellationToken = default)
    {
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Clients.AnyAsync(c => c.Id == id, cancellationToken);
}
