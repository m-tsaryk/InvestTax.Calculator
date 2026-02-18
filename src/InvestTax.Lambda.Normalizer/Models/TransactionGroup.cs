namespace InvestTax.Lambda.Normalizer.Models;

/// <summary>
/// Represents a group of transactions for a single ISIN
/// </summary>
public class TransactionGroup
{
    /// <summary>International Securities Identification Number</summary>
    public string ISIN { get; set; } = string.Empty;
    
    /// <summary>Stock ticker symbol</summary>
    public string Ticker { get; set; } = string.Empty;
    
    /// <summary>List of all transactions for this ISIN, sorted by date</summary>
    public List<NormalizedTransaction> Transactions { get; set; } = new();
}
