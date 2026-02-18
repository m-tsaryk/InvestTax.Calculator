using InvestTax.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace InvestTax.Core.Models;

/// <summary>
/// Represents a single buy or sell transaction
/// </summary>
public class Transaction
{
    /// <summary>Buy or Sell action</summary>
    [Required]
    public TransactionAction Action { get; set; }
    
    /// <summary>Transaction timestamp (UTC)</summary>
    [Required]
    public DateTime Time { get; set; }
    
    /// <summary>ISIN code of the security</summary>
    [Required]
    [StringLength(12, MinimumLength = 12)]
    public string ISIN { get; set; } = string.Empty;
    
    /// <summary>Ticker symbol</summary>
    public string? Ticker { get; set; }
    
    /// <summary>Security name or notes</summary>
    public string? Name { get; set; }
    
    /// <summary>Transaction ID from broker</summary>
    [Required]
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>Number of shares</summary>
    [Required]
    [Range(0.000001, double.MaxValue)]
    public decimal Shares { get; set; }
    
    /// <summary>Price per share in original currency</summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal PricePerShare { get; set; }
    
    /// <summary>Currency of price per share</summary>
    [Required]
    public Currency PriceCurrency { get; set; }
    
    /// <summary>Exchange rate used by broker (if provided)</summary>
    public decimal? BrokerExchangeRate { get; set; }
    
    /// <summary>Transaction result/total in result currency</summary>
    [Required]
    public decimal Result { get; set; }
    
    /// <summary>Currency of result</summary>
    [Required]
    public Currency ResultCurrency { get; set; }
    
    /// <summary>Final total amount in total currency</summary>
    [Required]
    public decimal Total { get; set; }
    
    /// <summary>Currency of total</summary>
    [Required]
    public Currency TotalCurrency { get; set; }
    
    /// <summary>NBP exchange rate for trade date (PLN per unit of foreign currency)</summary>
    public decimal? NBPExchangeRate { get; set; }
    
    /// <summary>Total value in PLN (calculated)</summary>
    public decimal? TotalPLN { get; set; }
    
    /// <summary>Transaction costs/commissions in PLN</summary>
    public decimal TransactionCostsPLN { get; set; }
}
