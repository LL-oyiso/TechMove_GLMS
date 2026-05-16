using System.Text.Json;
using System.Text.Json.Serialization;

namespace GLMS_Monolith.Services;

public class ExchangeRateApiProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ExchangeRateApiProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["FxApi:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("FX API key is missing. Set FxApi:ApiKey in configuration.");
        }

        // ExchangeRate-API format:
        // https://v6.exchangerate-api.com/v6/{API_KEY}/latest/USD
        var url = $"{apiKey}/latest/USD";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"FX API error: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ExchangeRateApiResponse>(stream, cancellationToken: cancellationToken);

        if (payload == null || !string.Equals(payload.Result, "success", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("FX API response indicates failure.");
        }

        if (payload.ConversionRates == null || !payload.ConversionRates.TryGetValue("ZAR", out var zarRate) || zarRate <= 0)
        {
            throw new InvalidOperationException("ZAR conversion rate missing or invalid.");
        }

        return zarRate;
    }

    private sealed class ExchangeRateApiResponse
    {
        [JsonPropertyName("result")]
        public string? Result { get; set; }

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, decimal>? ConversionRates { get; set; }
    }
}