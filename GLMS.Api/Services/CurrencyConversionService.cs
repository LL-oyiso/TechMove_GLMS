namespace GLMS.Api.Services;

public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly IExchangeRateProvider _exchangeRateProvider;

    public CurrencyConversionService(IExchangeRateProvider exchangeRateProvider)
    {
        _exchangeRateProvider = exchangeRateProvider;
    }

    public async Task<CurrencyConversionResult> ConvertUsdToZarAsync(decimal usdAmount, CancellationToken cancellationToken = default)
    {
        if (usdAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(usdAmount), "USD amount must be greater than zero.");
        }

        var rate = await _exchangeRateProvider.GetUsdToZarRateAsync(cancellationToken);
        var zar = Math.Round(usdAmount * rate, 2, MidpointRounding.AwayFromZero);

        return new CurrencyConversionResult(
            UsdAmount: usdAmount,
            RateUsed: rate,
            ZarAmount: zar
        );
    }
}
