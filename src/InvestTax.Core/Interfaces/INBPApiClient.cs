using InvestTax.Core.Enums;

namespace InvestTax.Core.Interfaces;

/// <summary>
/// Client for NBP exchange rate API
/// </summary>
public interface INBPApiClient
{
    /// <summary>
    /// Get exchange rate for a specific date and currency
    /// </summary>
    /// <param name="currency">Currency code (USD, EUR, etc.)</param>
    /// <param name="date">Date for exchange rate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exchange rate (PLN per unit of foreign currency)</returns>
    Task<decimal> GetExchangeRateAsync(Currency currency, DateOnly date, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get exchange rates for multiple dates (batch)
    /// </summary>
    Task<Dictionary<DateOnly, decimal>> GetExchangeRatesAsync(Currency currency, List<DateOnly> dates, CancellationToken cancellationToken = default);
}
