using GLMS.Api.Models;

namespace GLMS.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
