using System.Net;
using System.Net.Http.Json;
using GLMS.Shared.Dtos;

namespace GLMS_Monolith.Services.Api;

public class ServiceRequestsApi : IServiceRequestsApi
{
    private readonly HttpClient _http;

    public ServiceRequestsApi(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<ServiceRequestDto>> GetAllAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync("api/servicerequests", ct);
        await response.EnsureApiSuccessAsync(ct);
        return await response.Content.ReadFromJsonAsync<List<ServiceRequestDto>>(cancellationToken: ct) ?? new List<ServiceRequestDto>();
    }

    public async Task<ServiceRequestDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/servicerequests/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await response.EnsureApiSuccessAsync(ct);
        return await response.Content.ReadFromJsonAsync<ServiceRequestDto>(cancellationToken: ct);
    }

    public async Task<ServiceRequestDto> CreateAsync(ServiceRequestInputDto input, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/servicerequests", input, ct);
        await response.EnsureApiSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<ServiceRequestDto>(cancellationToken: ct))!;
    }

    public async Task UpdateAsync(int id, ServiceRequestInputDto input, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/servicerequests/{id}", input, ct);
        await response.EnsureApiSuccessAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/servicerequests/{id}", ct);
        await response.EnsureApiSuccessAsync(ct);
    }

    public async Task<CurrencyEstimateDto?> GetEstimateAsync(decimal usdAmount, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/servicerequests/estimate?usdAmount={usdAmount}", ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CurrencyEstimateDto>(cancellationToken: ct);
    }
}
