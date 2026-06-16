using System.Net;
using System.Net.Http.Json;
using GLMS.Shared.Dtos;

namespace GLMS_Monolith.Services.Api;

public class AuthApi : IAuthApi
{
    private readonly HttpClient _http;

    public AuthApi(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        await response.EnsureApiSuccessAsync(ct);
        return await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
    }
}
