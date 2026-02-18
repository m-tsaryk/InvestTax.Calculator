using CsvHelper.Configuration.Attributes;

namespace InvestTax.Lambda.Validator.Models;

/// <summary>
/// Represents a single row from the uploaded CSV file
/// </summary>
public class CsvRow
{
    [Name("Action")]
    public string Action { get; set; } = string.Empty;
    
    [Name("Time")]
    public string Time { get; set; } = string.Empty;
    
    [Name("ISIN")]
    public string ISIN { get; set; } = string.Empty;
    
    [Name("Ticker")]
    public string Ticker { get; set; } = string.Empty;
    
    [Name("Name")]
    public string Name { get; set; } = string.Empty;
    
    [Name("No. of shares")]
    public string NoOfShares { get; set; } = string.Empty;
    
    [Name("Price / share")]
    public string PricePerShare { get; set; } = string.Empty;
    
    [Name("Currency (Price / share)")]
    public string CurrencySymbol { get; set; } = string.Empty;
    
    [Name("Exchange rate")]
    public string ExchangeRate { get; set; } = string.Empty;
    
    [Name("Result")]
    public string Result { get; set; } = string.Empty;
    
    [Name("Total")]
    public string Total { get; set; } = string.Empty;
    
    [Name("Notes")]
    public string Notes { get; set; } = string.Empty;
}
