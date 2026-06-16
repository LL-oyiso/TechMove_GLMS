using System.Net.Http.Headers;

namespace GLMS_Monolith.Services.Api;

// Attaches the signed-in user's JWT (stored in session at login) to every outgoing API request.
public class AuthTokenHandler : DelegatingHandler
{
    public const string TokenSessionKey = "GLMS_AccessToken";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString(TokenSessionKey);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
