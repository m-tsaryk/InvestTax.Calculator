namespace InvestTax.Lambda.NBPClient.Models;

/// <summary>
/// Represents a unique currency-date pair requiring an exchange rate
/// </summary>
public class RateRequest
{
    /// <summary>Currency code (USD, EUR, etc.)</summary>
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>Transaction date</summary>
    public DateOnly Date { get; set; }
}
