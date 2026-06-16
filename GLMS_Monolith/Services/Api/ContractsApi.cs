using System.Net;
using System.Net.Http.Json;
using GLMS.Shared.Dtos;
using GLMS.Shared.Enums;

namespace GLMS_Monolith.Services.Api;

public class ContractsApi : IContractsApi
{
    private readonly HttpClient _http;

    public ContractsApi(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<ContractDto>> GetAllAsync(ContractStatus? status, DateTime? startFrom, DateTime? endTo, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (status.HasValue) query.Add($"status={status.Value}");
        if (startFrom.HasValue) query.Add($"startFrom={startFrom.Value:yyyy-MM-dd}");
        if (endTo.HasValue) query.Add($"endTo={endTo.Value:yyyy-MM-dd}");
        var url = "api/contracts" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);

        var response = await _http.GetAsync(url, ct);
        await response.EnsureApiSuccessAsync(ct);
        return await response.Content.ReadFromJsonAsync<List<ContractDto>>(cancellationToken: ct) ?? new List<ContractDto>();
    }

    public async Task<ContractDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/contracts/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await response.EnsureApiSuccessAsync(ct);
        return await response.Content.ReadFromJsonAsync<ContractDto>(cancellationToken: ct);
    }

    public async Task<ContractDto> CreateAsync(ContractInputDto input, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/contracts", input, ct);
        await response.EnsureApiSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<ContractDto>(cancellationToken: ct))!;
    }

    public async Task UpdateAsync(int id, ContractInputDto input, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/contracts/{id}", input, ct);
        await response.EnsureApiSuccessAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/contracts/{id}", ct);
        await response.EnsureApiSuccessAsync(ct);
    }

    public async Task UploadAgreementAsync(int id, Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "application/pdf" : contentType);
        content.Add(fileContent, "file", fileName);

        var response = await _http.PostAsync($"api/contracts/{id}/agreement", content, ct);
        await response.EnsureApiSuccessAsync(ct);
    }

    public async Task<AgreementDownload?> DownloadAgreementAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/contracts/{id}/agreement", HttpCompletionOption.ResponseHeadersRead, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await response.EnsureApiSuccessAsync(ct);

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf";
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                       ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                       ?? $"contract-{id}.pdf";
        var stream = await response.Content.ReadAsStreamAsync(ct);
        return new AgreementDownload(stream, contentType, fileName);
    }
}
