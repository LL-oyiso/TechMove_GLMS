using GLMS.Api.Data;
using GLMS.Api.Models;
using GLMS.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly GlmsDbContext _context;

    public ContractRepository(GlmsDbContext context)
    {
        _context = context;
    }

    public async Task<List<Contract>> GetAllAsync(ContractStatus? status, DateTime? startFrom, DateTime? endTo, CancellationToken cancellationToken = default)
    {
        var query = _context.Contracts
            .Include(c => c.Client)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (startFrom.HasValue)
        {
            query = query.Where(c => c.StartDate >= startFrom.Value.Date);
        }

        if (endTo.HasValue)
        {
            query = query.Where(c => c.EndDate <= endTo.Value.Date);
        }

        return await query.OrderByDescending(c => c.Id).ToListAsync(cancellationToken);
    }

    public async Task<Contract?> GetByIdAsync(int id, bool includeClient = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Contracts.AsQueryable();
        if (includeClient)
        {
            query = query.Include(c => c.Client);
        }
        return await query.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Contract> AddAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync(cancellationToken);
        return contract;
    }

    public async Task UpdateAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Contracts.AnyAsync(c => c.Id == id, cancellationToken);
}
