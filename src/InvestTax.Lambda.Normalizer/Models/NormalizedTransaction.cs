using InvestTax.Core.Enums;

namespace InvestTax.Lambda.Normalizer.Models;

/// <summary>
/// Represents a single normalized transaction
/// </summary>
public class NormalizedTransaction
{
    /// <summary>Transaction sequence ID</summary>
    public int Id { get; set; }
    
    /// <summary>Transaction type (Buy or Sell)</summary>
    public TransactionAction Action { get; set; }
    
    /// <summary>Transaction date (normalized to Europe/Warsaw timezone)</summary>
    public DateTime TransactionDate { get; set; }
    
    /// <summary>International Securities Identification Number</summary>
    public string ISIN { get; set; } = string.Empty;
    
    /// <summary>Stock ticker symbol</summary>
    public string Ticker { get; set; } = string.Empty;
    
    /// <summary>Security name</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Number of shares in transaction</summary>
    public decimal Shares { get; set; }
    
    /// <summary>Price per share in original currency</summary>
    public decimal PricePerShare { get; set; }
    
    /// <summary>Currency of the transaction (e.g., USD, EUR)</summary>
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>Exchange rate to PLN (if already provided in source data)</summary>
    public decimal ExchangeRate { get; set; }
    
    /// <summary>Total transaction value in original currency</summary>
    public decimal Total { get; set; }
    
    /// <summary>Optional notes or comments</summary>
    public string Notes { get; set; } = string.Empty;
}
