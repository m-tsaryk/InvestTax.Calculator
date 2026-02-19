namespace InvestTax.Core.Models;

/// <summary>
/// Result of FIFO tax calculation for a single sell matched to buy(s)
/// </summary>
public class TaxCalculation
{
    /// <summary>ISIN of the security</summary>
    public string ISIN { get; set; } = string.Empty;
    
    /// <summary>Sell transaction</summary>
    public Transaction SellTransaction { get; set; } = null!;
    
    /// <summary>Matched buy transactions (FIFO order)</summary>
    public List<MatchedBuy> MatchedBuys { get; set; } = new();
    
    /// <summary>Total cost basis in PLN</summary>
    public decimal CostBasisPLN { get; set; }
    
    /// <summary>Total proceeds in PLN</summary>
    public decimal ProceedsPLN { get; set; }
    
    /// <summary>Capital gain/loss in PLN</summary>
    public decimal GainLossPLN { get; set; }
    
    /// <summary>Whether this is a gain or loss</summary>
    public bool IsGain => GainLossPLN > 0;
}

/// <summary>
/// Represents a buy transaction matched to a sell via FIFO
/// </summary>
public class MatchedBuy
{
    /// <summary>Buy transaction</summary>
    public Transaction BuyTransaction { get; set; } = null!;
    
    /// <summary>Number of shares matched from this buy</summary>
    public decimal SharesMatched { get; set; }
    
    /// <summary>Cost basis for matched shares in PLN</summary>
    public decimal CostBasisPLN { get; set; }
}
