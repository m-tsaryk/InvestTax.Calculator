namespace InvestTax.Core.Models;

/// <summary>
/// Response from NBP API for exchange rates
/// </summary>
public class NBPExchangeRateResponse
{
    public string Table { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public List<NBPRate> Rates { get; set; } = new();
}

public class NBPRate
{
    public string No { get; set; } = string.Empty;
    public string EffectiveDate { get; set; } = string.Empty;
    public decimal Mid { get; set; }
}
