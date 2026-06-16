using FluentAssertions;
using GLMS.Api.Services;

namespace Glms_Monolith_Test;

public class CurrencyConversionServiceTests
{
    [Fact]
    public async Task ConvertUsdToZarAsync_WithValidAmount_ReturnsRoundedResult()
    {
        var provider = new StubExchangeRateProvider(18.567891m);
        var service = new CurrencyConversionService(provider);

        var result = await service.ConvertUsdToZarAsync(100m);

        result.UsdAmount.Should().Be(100m);
        result.RateUsed.Should().Be(18.567891m);
        result.ZarAmount.Should().Be(1856.79m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-500)]
    public async Task ConvertUsdToZarAsync_WithNonPositiveAmount_Throws(decimal usdAmount)
    {
        var provider = new StubExchangeRateProvider(18.5m);
        var service = new CurrencyConversionService(provider);

        var act = () => service.ConvertUsdToZarAsync(usdAmount);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ConvertUsdToZarAsync_WhenProviderFails_PropagatesException()
    {
        var provider = new FailingExchangeRateProvider();
        var service = new CurrencyConversionService(provider);

        var act = () => service.ConvertUsdToZarAsync(100m);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*provider unavailable*");
    }

    private sealed class StubExchangeRateProvider(decimal rate) : IExchangeRateProvider
    {
        public Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default) => Task.FromResult(rate);
    }

    private sealed class FailingExchangeRateProvider : IExchangeRateProvider
    {
        public Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("provider unavailable");
    }
}
