using GLMS.Shared.Dtos;

namespace GLMS_Monolith.Services.Api;

public interface IAuthApi
{
    // Returns the auth response on success, or null when credentials are rejected (401).
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
