namespace InvestTax.Lambda.NBPClient.Models;

/// <summary>
/// Map of exchange rates indexed by currency and date
/// </summary>
public class RateMap
{
    /// <summary>
    /// Dictionary of rates: Key format is "CURRENCY_YYYY-MM-DD", Value is the PLN exchange rate
    /// </summary>
    public Dictionary<string, decimal> Rates { get; set; } = new();
}
