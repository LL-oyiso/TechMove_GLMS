using GLMS.Api.Data;
using GLMS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly GlmsDbContext _context;

    public UserRepository(GlmsDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
}
