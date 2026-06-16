namespace GLMS.Api.Services;

public interface IExchangeRateProvider
{
    Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default);
}
