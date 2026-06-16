using System.Net;
using System.Net.Http.Json;
using GLMS.Shared.Dtos;

namespace GLMS_Monolith.Services.Api;

public class ClientsApi : IClientsApi
{
    private readonly HttpClient _http;

    public ClientsApi(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<ClientDto>> GetAllAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync("api/clients", ct);
        await response.EnsureApiSuccessAsync(ct);
        return await response.Content.ReadFromJsonAsync<List<ClientDto>>(cancellationToken: ct) ?? new List<ClientDto>();
    }

    public async Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/clients/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await response.EnsureApiSuccessAsync(ct);
        return await response.Content.ReadFromJsonAsync<ClientDto>(cancellationToken: ct);
    }

    public async Task<ClientDto> CreateAsync(ClientInputDto input, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/clients", input, ct);
        await response.EnsureApiSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<ClientDto>(cancellationToken: ct))!;
    }

    public async Task UpdateAsync(int id, ClientInputDto input, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/clients/{id}", input, ct);
        await response.EnsureApiSuccessAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/clients/{id}", ct);
        await response.EnsureApiSuccessAsync(ct);
    }
}
