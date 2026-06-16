namespace GLMS.Api.Services;

public interface ICurrencyConversionService
{
    Task<CurrencyConversionResult> ConvertUsdToZarAsync(decimal usdAmount, CancellationToken cancellationToken = default);
}

public record CurrencyConversionResult(decimal UsdAmount, decimal RateUsed, decimal ZarAmount);
