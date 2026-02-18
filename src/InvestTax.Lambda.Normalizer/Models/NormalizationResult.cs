namespace InvestTax.Lambda.Normalizer.Models;

/// <summary>
/// Result of the normalization process
/// </summary>
public class NormalizationResult
{
    /// <summary>S3 key where normalized JSON file is stored</summary>
    public string NormalizedFileKey { get; set; } = string.Empty;
    
    /// <summary>Dictionary of transaction groups indexed by ISIN</summary>
    public Dictionary<string, TransactionGroup> TransactionGroups { get; set; } = new();
    
    /// <summary>Total number of transactions processed</summary>
    public int TotalTransactions { get; set; }
}
