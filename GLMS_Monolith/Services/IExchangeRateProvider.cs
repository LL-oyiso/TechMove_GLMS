namespace GLMS_Monolith.Services;

public interface IExchangeRateProvider
{
    Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default);
}